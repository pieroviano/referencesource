//------------------------------------------------------------------------------
// <copyright file="EnumConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
#if !NETSTANDARD
    using System.Runtime.Remoting;
#endif
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Provides a type converter to convert <see cref='System.Enum'/>
    /// objects to and from various
    /// other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class EnumConverter : TypeConverter {
        /// <devdoc>
        ///    <para>
        ///       Provides a <see cref='System.ComponentModel.TypeConverter.StandardValuesCollection'/> that specifies the
        ///       possible values for the enumeration.
        ///    </para>
        /// </devdoc>
        private StandardValuesCollection values;
        /// <devdoc>
        ///    <para>
        ///       Specifies
        ///       the
        ///       type of the enumerator this converter is
        ///       associated with.
        ///    </para>
        /// </devdoc>
        private Type type;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.EnumConverter'/> class for the given
        ///       type.
        ///    </para>
        /// </devdoc>
        public EnumConverter(Type type) {
            this.type = type;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected Type EnumType {
            get {
                return type;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected StandardValuesCollection Values {
            get {
                return values;
            }
            set {
                values = value;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter
        ///       can convert an object in the given source type to an enumeration object using
        ///       the specified context.</para>
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string) || sourceType == typeof(Enum[])) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object to the given destination type using the context.</para>
        /// </devdoc>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(Enum[])) {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        /// <devdoc>
        /// <para>Gets an <see cref='System.Collections.IComparer'/>
        /// interface that can
        /// be used to sort the values of the enumerator.</para>
        /// </devdoc>
        protected virtual IComparer Comparer {
            get {
                return InvariantComparer.Default;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Converts the specified value object to an enumeration object.</para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                try {
                    string strValue = (string)value;
                    if (strValue.IndexOf(',') != -1) {
                        long convertedValue = 0;
                        string[] values = strValue.Split(new char[] {','});
                        foreach(string v in values) {
                            convertedValue |= Convert.ToInt64((Enum)Enum.Parse(type, v, true), culture);
                        }
                        return Enum.ToObject(type, convertedValue);
                    }
                    else {
                        return Enum.Parse(type, strValue, true);
                    }
                }
                catch (Exception e) {
                    throw new FormatException(SR.GetString(SR.ConvertInvalidPrimitive, (string)value, type.Name), e);
                }
            }
            else if (value is Enum[]) {
                long finalValue = 0;
                foreach(Enum e in (Enum[])value) {
                    finalValue |= Convert.ToInt64(e, culture);
                }
                return Enum.ToObject(type, finalValue);
            }
            return base.ConvertFrom(context, culture, value);
        }
    
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Converts the given
        ///       value object to the
        ///       specified destination type.</para>
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]  // Keep call to Enum.IsDefined
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string) && value != null) {
                // Raise an argument exception if the value isn't defined and if
                // the enum isn't a flags style.
                //
                Type underlyingType = Enum.GetUnderlyingType(type);
                if (value is IConvertible && value.GetType() != underlyingType) {
                    value = ((IConvertible)value).ToType(underlyingType, culture);
                }
                if (!type.IsDefined(typeof(FlagsAttribute), false) && !Enum.IsDefined(type, value)) {
                    throw new ArgumentException(SR.GetString(SR.EnumConverterInvalidValue, value.ToString(), type.Name));
                }
                
                return Enum.Format(type, value, "G");
            }
            if (destinationType == typeof(InstanceDescriptor) && value != null) {
                string enumName = ConvertToInvariantString(context, value);
                
                if (type.IsDefined(typeof(FlagsAttribute), false) && enumName.IndexOf(',') != -1) {
                    // This is a flags enum, and there is no one flag
                    // that covers the value.  Instead, convert the
                    // value to the underlying type and invoke
                    // a ToObject call on enum.
                    //
                    Type underlyingType = Enum.GetUnderlyingType(type);
                    if (value is IConvertible) {
                        object convertedValue = ((IConvertible)value).ToType(underlyingType, culture);
                        
                        MethodInfo method = typeof(Enum).GetMethod("ToObject", new Type[] {typeof(Type), underlyingType});
                        if (method != null) {
                            return new InstanceDescriptor(method, new object[] {type, convertedValue});
                        }
                    }
                }
                else {
                    FieldInfo info = type.GetField(enumName);
                    if (info != null) {
                        return new InstanceDescriptor(info, null);
                    }
                }
            }
            if (destinationType == typeof(Enum[]) && value != null) {
                if (type.IsDefined(typeof(FlagsAttribute), false)) {
                    List<Enum> flagValues = new List<Enum>();

                    Array objValues = Enum.GetValues(type);
                    long[] ulValues = new long[objValues.Length];
                    for(int idx = 0; idx < objValues.Length; idx++) {
                        ulValues[idx] = Convert.ToInt64((Enum)objValues.GetValue(idx), culture);
                    }

                    long longValue = Convert.ToInt64((Enum)value, culture);
                    bool valueFound = true;
                    while(valueFound) {
                        valueFound = false;
                        foreach(long ul in ulValues) {
                            if ((ul != 0 && (ul & longValue) == ul) || ul == longValue) {
                                flagValues.Add((Enum)Enum.ToObject(type, ul));
                                valueFound = true;
                                longValue &= ~ul;
                                break;
                            }
                        }

                        if (longValue == 0) {
                            break;
                        }
                    }

                    if (!valueFound && longValue != 0) {
                        flagValues.Add((Enum)Enum.ToObject(type, longValue));
                    }

                    return flagValues.ToArray();
                }
                else {
                    return new Enum[] {(Enum)Enum.ToObject(type, value)};
                }
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a collection of standard values for the data type this validator is
        ///       designed for.</para>
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (values == null) {
                // We need to get the enum values in this rather round-about way so we can filter
                // out fields marked Browsable(false). Note that if multiple fields have the same value,
                // the behavior is undefined, since what we return are just enum values, not names.

                Type reflectType = TypeDescriptor.GetReflectionType(type);
                if (reflectType == null) {
                    reflectType = type;
                }

                FieldInfo[] fields = reflectType.GetFields(BindingFlags.Public | BindingFlags.Static);
                ArrayList objValues = null;

                if (fields != null && fields.Length > 0) {
                    objValues = new ArrayList(fields.Length);
                }

                if (objValues != null) {
                    foreach (FieldInfo field in fields) {
                        BrowsableAttribute browsableAttr = null;
                        foreach (Attribute attr in field.GetCustomAttributes(typeof(BrowsableAttribute), false)) {
                            browsableAttr = attr as BrowsableAttribute;
                        }
                    
                        if (browsableAttr == null || browsableAttr.Browsable) {
                            object value = null;
    
                            try {
                                if (field.Name != null) {
                                    value = Enum.Parse(type, field.Name);
                                }
                            }
                            catch (ArgumentException) {
                                // Hmm, for some reason, the parse threw. Let us ignore this value.
                            }
    
                            if (value != null) {
                                objValues.Add(value);
                            }
                        }
                    }
                    
                    IComparer comparer = Comparer;
                    if (comparer != null) {
                        objValues.Sort(comparer);
                    }
                }

                Array arr = (objValues != null) ? objValues.ToArray() : null;
                values = new StandardValuesCollection(arr);
            }
            return values;
        }
    
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether the list of standard values returned from
        ///    <see cref='System.ComponentModel.TypeConverter.GetStandardValues'/> 
        ///    is an exclusive list using the specified context.</para>
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return !type.IsDefined(typeof(FlagsAttribute), false);
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object
        ///       supports a standard set of values that can be picked
        ///       from a list using the specified context.</para>
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether the given object value is valid for this type.</para>
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]  // Keep call to Enum.IsDefined
        public override bool IsValid(ITypeDescriptorContext context, object value) {
            return Enum.IsDefined(type, value);
        }
    }
}

