//------------------------------------------------------------------------------
// <copyright file="TypeConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.Configuration;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
#if !NETSTANDARD
    using System.Runtime.Remoting;
#endif
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Converts the value of an object into a different data type.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class TypeConverter {

        private const string s_UseCompatibleTypeConverterBehavior = "UseCompatibleTypeConverterBehavior";
        private static volatile bool useCompatibleTypeConversion = false;
        private static volatile bool firstLoadAppSetting = true;
        private static object loadAppSettingLock = new Object();

        private static bool UseCompatibleTypeConversion {
            get {
                if (firstLoadAppSetting) {
                    lock (loadAppSettingLock) {
                        if (firstLoadAppSetting) {
                            string useCompatibleConfig = ConfigurationManager.AppSettings[s_UseCompatibleTypeConverterBehavior];

                            try {
                                if (!String.IsNullOrEmpty(useCompatibleConfig)) {
                                    useCompatibleTypeConversion = bool.Parse(useCompatibleConfig.Trim());
                                }
                            }
                            catch {
                                // we get any exception, then eat out the exception, and use the new TypeConverter.
                                useCompatibleTypeConversion = false;
                            }

                            firstLoadAppSetting = false;
                        }
                    }
                }
                return useCompatibleTypeConversion;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can convert an object in the
        ///       given source type to the native type of the converter.</para>
        /// </devdoc>
        public bool CanConvertFrom(Type sourceType) {
            return CanConvertFrom(null, sourceType);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object in the given source type to the native type of the converter
        ///       using the context.</para>
        /// </devdoc>
        public virtual bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(InstanceDescriptor)) {
                return true;
            }
            return false;
        }
        
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object to the given destination type using the context.</para>
        /// </devdoc>
        public bool CanConvertTo(Type destinationType) {
            return CanConvertTo(null, destinationType);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object to the given destination type using the context.</para>
        /// </devdoc>
        public virtual bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return (destinationType == typeof(string));
        }

        /// <devdoc>
        ///    <para>Converts the given value
        ///       to the converter's native type.</para>
        /// </devdoc>
        public object ConvertFrom(object value) {
            return ConvertFrom(null, CultureInfo.CurrentCulture, value);
        }

        /// <devdoc>
        ///    <para>Converts the given object to the converter's native type.</para>
        /// </devdoc>
        public virtual object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            InstanceDescriptor id = value as InstanceDescriptor;
            if (id != null) {
                return id.Invoke();
            }
            throw GetConvertFromException(value);
        }

        /// <devdoc>
        ///    Converts the given string to the converter's native type using the invariant culture.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public object ConvertFromInvariantString(string text) {
            return ConvertFromString(null, CultureInfo.InvariantCulture, text);
        }

        /// <devdoc>
        ///    Converts the given string to the converter's native type using the invariant culture.
        /// </devdoc>
        public object ConvertFromInvariantString(ITypeDescriptorContext context, string text) {
            return ConvertFromString(context, CultureInfo.InvariantCulture, text);
        }

        /// <devdoc>
        ///    <para>Converts the specified text into an object.</para>
        /// </devdoc>
        public object ConvertFromString(string text) {
            return ConvertFrom(null, null, text);
        }

        /// <devdoc>
        ///    <para>Converts the specified text into an object.</para>
        /// </devdoc>
        public object ConvertFromString(ITypeDescriptorContext context, string text) {
            return ConvertFrom(context, CultureInfo.CurrentCulture, text);
        }

        /// <devdoc>
        ///    <para>Converts the specified text into an object.</para>
        /// </devdoc>
        public object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text) {
            return ConvertFrom(context, culture, text);
        }

        /// <devdoc>
        ///    <para>Converts the given
        ///       value object to the specified destination type using the arguments.</para>
        /// </devdoc>
        public object ConvertTo(object value, Type destinationType) {
            return ConvertTo(null, null, value, destinationType);
        }

        /// <devdoc>
        ///    <para>Converts the given value object to
        ///       the specified destination type using the specified context and arguments.</para>
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // keep CultureInfo
        public virtual object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string)) {
                if (value == null) {
                    return String.Empty;
                }

                // Pre-whidbey we just did a ToString() here.  To minimize the chance of a breaking change we
                // still send requests for the CurrentCulture to ToString() (which should return the same).
                if(culture != null && culture != CultureInfo.CurrentCulture) {
                    // VSWhidbey 75433 - If the object is IFormattable, use this interface to convert to string
                    // so we use the specified culture rather than the CurrentCulture like object.ToString() does.
                    IFormattable formattable = value as IFormattable;
                    if(formattable != null) {
                        return formattable.ToString(/* format = */ null, /* formatProvider = */ culture);
                    }
                }
                return value.ToString();
            }
            throw GetConvertToException(value, destinationType);
        }

        /// <devdoc>
        ///    <para>Converts the specified value to a culture-invariant string representation.</para>
        /// </devdoc>
        public string ConvertToInvariantString(object value) {
            return ConvertToString(null, CultureInfo.InvariantCulture, value);
        }

        /// <devdoc>
        ///    <para>Converts the specified value to a culture-invariant string representation.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public string ConvertToInvariantString(ITypeDescriptorContext context, object value) {
            return ConvertToString(context, CultureInfo.InvariantCulture, value);
        }

        /// <devdoc>
        ///    <para>Converts the specified value to a string representation.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public string ConvertToString(object value) {
            return (string)ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
        }

        /// <devdoc>
        ///    <para>Converts the specified value to a string representation.</para>
        /// </devdoc>
        public string ConvertToString(ITypeDescriptorContext context, object value) {
            return (string)ConvertTo(context, CultureInfo.CurrentCulture, value, typeof(string));
        }

        /// <devdoc>
        ///    <para>Converts the specified value to a string representation.</para>
        /// </devdoc>
        public string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return (string)ConvertTo(context, culture, value, typeof(string));
        }

        /// <devdoc>
        /// <para>Re-creates an <see cref='System.Object'/> given a set of property values for the object.</para>
        /// </devdoc>
        public object CreateInstance(IDictionary propertyValues) {
            return CreateInstance(null, propertyValues);
        }

        /// <devdoc>
        /// <para>Re-creates an <see cref='System.Object'/> given a set of property values for the
        ///    object.</para>
        /// </devdoc>
        public virtual object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues) {
            return null;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a suitable exception to throw when a conversion cannot
        ///       be performed.
        ///    </para>
        /// </devdoc>
        protected Exception GetConvertFromException(object value) {
            string valueTypeName;

            if (value == null) {
                valueTypeName = SR.GetString(SR.ToStringNull);
            }
            else {
                valueTypeName = value.GetType().FullName;
            }

            throw new NotSupportedException(SR.GetString(SR.ConvertFromException, GetType().Name, valueTypeName));
        }

        /// <devdoc>
        ///    <para>Retrieves a suitable exception to throw when a conversion cannot
        ///       be performed.</para>
        /// </devdoc>
        protected Exception GetConvertToException(object value, Type destinationType) {
            string valueTypeName;

            if (value == null) {
                valueTypeName = SR.GetString(SR.ToStringNull);
            }
            else {
                valueTypeName = value.GetType().FullName;
            }

            throw new NotSupportedException(SR.GetString(SR.ConvertToException, GetType().Name, valueTypeName, destinationType.FullName));
        }
        
        /// <devdoc>
        ///    <para>Gets a value indicating whether changing a value on this 
        ///       object requires a call to <see cref='System.ComponentModel.TypeConverter.CreateInstance'/>
        ///       to create a new value.</para>
        /// </devdoc>
        public bool GetCreateInstanceSupported() {
            return GetCreateInstanceSupported(null);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether changing a value on this object requires a 
        ///       call to <see cref='System.ComponentModel.TypeConverter.CreateInstance'/> to create a new value,
        ///       using the specified context.</para>
        /// </devdoc>
        public virtual bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
            return false;
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for the type of array specified by the value
        ///       parameter.</para>
        /// </devdoc>
        public PropertyDescriptorCollection GetProperties(object value) {
            return GetProperties(null, value);
        }

        /// <devdoc>
        ///    <para>Gets a collection of
        ///       properties for the type of array specified by the value parameter using the specified
        ///       context.</para>
        /// </devdoc>
        public PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value) {
            return GetProperties(context, value, new Attribute[] {BrowsableAttribute.Yes});
        }  
        
        /// <devdoc>
        ///    <para>Gets a collection of properties for
        ///       the type of array specified by the value parameter using the specified context and
        ///       attributes.</para>
        /// </devdoc>
        public virtual PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
            return null;
        }
       
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether this object supports properties.
        ///    </para>
        /// </devdoc>
        public bool GetPropertiesSupported() {
            return GetPropertiesSupported(null);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object supports properties using the
        ///       specified context.</para>
        /// </devdoc>
        public virtual bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return false;
        }
        
        /// <devdoc>
        ///    <para> Gets a collection of standard values for the data type this type
        ///       converter is designed for.</para>
        /// </devdoc>
        public ICollection GetStandardValues() {
            return GetStandardValues(null);
        }

        /// <devdoc>
        ///    <para>Gets a collection of standard values for the data type this type converter is
        ///       designed for.</para>
        /// </devdoc>
        public virtual StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return null;
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether the collection of standard values returned from
        ///    <see cref='System.ComponentModel.TypeConverter.GetStandardValues'/> is an exclusive list. </para>
        /// </devdoc>
        public bool GetStandardValuesExclusive() {
            return GetStandardValuesExclusive(null);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether the collection of standard values returned from
        ///    <see cref='System.ComponentModel.TypeConverter.GetStandardValues'/> is an exclusive 
        ///       list of possible values, using the specified context.</para>
        /// </devdoc>
        public virtual bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether this object supports a standard set of values
        ///       that can be picked from a list.
        ///    </para>
        /// </devdoc>
        public bool GetStandardValuesSupported() {
            return GetStandardValuesSupported(null);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object
        ///       supports a standard set of values that can be picked
        ///       from a list using the specified context.</para>
        /// </devdoc>
        public virtual bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return false;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       a value indicating whether the given value object is valid for this type.
        ///    </para>
        /// </devdoc>
        public bool IsValid(object value) {
            return IsValid(null, value);
        }

        /// <devdoc>
        ///    <para>Gets
        ///       a value indicating whether the given value object is valid for this type.</para>
        /// </devdoc>
        public virtual bool IsValid(ITypeDescriptorContext context, object value) {
            if (UseCompatibleTypeConversion) {
                return true;
            }

            bool isValid = true;
            try {
                // Because null doesn't have a type, so we couldn't pass this to CanConvertFrom.
                // Meanwhile, we couldn't silence null value here, such as type converter like
                // NullableConverter would consider null value as a valid value.
                if (value == null || CanConvertFrom(context, value.GetType())) {
                    ConvertFrom(context, CultureInfo.InvariantCulture, value);
                }
                else {
                    isValid = false;
                }
            }
            catch {
                isValid = false;
            }

            return isValid;
        }
        
        /// <devdoc>
        ///    <para>Sorts a collection of properties.</para>
        /// </devdoc>
        protected PropertyDescriptorCollection SortProperties(PropertyDescriptorCollection props, string[] names) {
            props.Sort(names);
            return props;
        }

        /// <devdoc>
        ///    <para>
        ///       An <see langword='abstract '/>
        ///       class that provides
        ///       properties for objects that do not have
        ///       properties.
        ///    </para>
        /// </devdoc>
        protected abstract class SimplePropertyDescriptor : PropertyDescriptor {
            private Type   componentType;
            private Type   propertyType;
        

            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.ComponentModel.TypeConverter.SimplePropertyDescriptor'/>
            ///       class.
            ///    </para>
            /// </devdoc>
            protected SimplePropertyDescriptor(Type componentType, string name, Type propertyType) : this(componentType, name, propertyType, new Attribute[0]) {
            }
            
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.ComponentModel.TypeConverter.SimplePropertyDescriptor'/> class.
            ///    </para>
            /// </devdoc>
            protected SimplePropertyDescriptor(Type componentType, string name, Type propertyType, Attribute[] attributes) : base(name, attributes) {
                this.componentType = componentType;
                this.propertyType = propertyType;
            }

            /// <devdoc>
            ///    <para>
            ///       Gets the type of the component this property description
            ///       is bound to.
            ///    </para>
            /// </devdoc>
            public override Type ComponentType {
                get {
                    return componentType;
                }
            }
                
            /// <devdoc>
            ///    <para>
            ///       Gets a
            ///       value indicating whether this property is read-only.
            ///    </para>
            /// </devdoc>
            public override bool IsReadOnly {
                get {
                    return Attributes.Contains(ReadOnlyAttribute.Yes);
                }
            }
    
            /// <devdoc>
            ///    <para>
            ///       Gets the type of the property.
            ///    </para>
            /// </devdoc>
            public override Type PropertyType {
                get {
                    return propertyType;
                }
            }
            
            /// <devdoc>
            ///    <para>Gets a value indicating whether resetting the component 
            ///       will change the value of the component.</para>
            /// </devdoc>
            public override bool CanResetValue(object component) {
                DefaultValueAttribute attr = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
                if (attr == null) {
                    return false;
                }
                return (attr.Value.Equals(GetValue(component)));
            }
            
            /// <devdoc>
            ///    <para>Resets the value for this property
            ///       of the component.</para>
            /// </devdoc>
            public override void ResetValue(object component) {
                DefaultValueAttribute attr = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
                if (attr != null) {
                    SetValue(component, attr.Value);
                }
            }
    
            /// <devdoc>
            ///    <para>Gets a value
            ///       indicating whether the value of this property needs to be persisted.</para>
            /// </devdoc>
            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }
        
        /// <devdoc>
        ///    <para>Represents a collection of values.</para>
        /// </devdoc>
        public class StandardValuesCollection : ICollection {
            private ICollection values;
            private Array       valueArray;
            
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.ComponentModel.TypeConverter.StandardValuesCollection'/>
            ///       class.
            ///    </para>
            /// </devdoc>
            public StandardValuesCollection(ICollection values) {
                if (values == null) {
                    values = new object[0];
                }
                
                Array a = values as Array;
                if (a != null) {
                    valueArray = a;
                }
                
                this.values = values;
            }
            
            /// <devdoc>
            ///    <para>
            ///       Gets the number of objects in the collection.
            ///    </para>
            /// </devdoc>
            public int Count {
                get {
                    if (valueArray != null) {
                        return valueArray.Length;
                    }
                    else {
                        return values.Count;
                    }
                }
            }
            
            /// <devdoc>
            ///    <para>Gets the object at the specified index number.</para>
            /// </devdoc>
            public object this[int index] {
                get {
                    if (valueArray != null) {
                        return valueArray.GetValue(index);
                    }
                    IList list = values as IList;
                    if (list != null) {
                        return list[index];
                    }
                    // No other choice but to enumerate the collection.
                    //
                    valueArray = new object[values.Count];
                    values.CopyTo(valueArray, 0);
                    return valueArray.GetValue(index);
                }
            }

            /// <devdoc>
            ///    <para>Copies the contents of this collection to an array.</para>
            /// </devdoc>
            public void CopyTo(Array array, int index) {
                values.CopyTo(array, index);
            }

            /// <devdoc>
            ///    <para>
            ///       Gets an enumerator for this collection.
            ///    </para>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return values.GetEnumerator();
            }
            
            /// <internalonly/>
            /// <devdoc>
            /// Retrieves the count of objects in the collection.
            /// </devdoc>
            int ICollection.Count {
                get {
                    return Count;
                }
            }

            /// <internalonly/>
            /// <devdoc>
            /// Determines if this collection is synchronized.
            /// The ValidatorCollection is not synchronized for
            /// speed.  Also, since it is read-only, there is
            /// no need to synchronize it.
            /// </devdoc>
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            /// <internalonly/>
            /// <devdoc>
            /// Retrieves the synchronization root for this
            /// collection.  Because we are not synchronized,
            /// this returns null.
            /// </devdoc>
            object ICollection.SyncRoot {
                get {
                    return null;
                }
            }

            /// <internalonly/>
            /// <devdoc>
            /// Copies the contents of this collection to an array.
            /// </devdoc>
            void ICollection.CopyTo(Array array, int index) {
                CopyTo(array, index);
            }

            /// <internalonly/>
            /// <devdoc>
            /// Retrieves a new enumerator that can be used to
            /// iterate over the values in this collection.
            /// </devdoc>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
    }
}

