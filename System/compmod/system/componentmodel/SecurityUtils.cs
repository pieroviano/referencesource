using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System
{
	internal static class SecurityUtils
	{
		private static volatile ReflectionPermission memberAccessPermission;

		private static volatile ReflectionPermission restrictedMemberAccessPermission;

		private static ReflectionPermission MemberAccessPermission
		{
			get
			{
				if (SecurityUtils.memberAccessPermission == null)
				{
					SecurityUtils.memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
				}
				return SecurityUtils.memberAccessPermission;
			}
		}

		private static ReflectionPermission RestrictedMemberAccessPermission
		{
			get
			{
				if (SecurityUtils.restrictedMemberAccessPermission == null)
				{
					SecurityUtils.restrictedMemberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess);
				}
				return SecurityUtils.restrictedMemberAccessPermission;
			}
		}

		internal static object ArrayCreateInstance(Type type, int length)
		{
			if (!type.IsVisible)
			{
				SecurityUtils.DemandReflectionAccess(type);
			}
			return Array.CreateInstance(type, length);
		}

		internal static object ConstructorInfoInvoke(ConstructorInfo ctor, object[] args)
		{
			Type declaringType = ctor.DeclaringType;
			if (declaringType != null && (!declaringType.IsVisible || !ctor.IsPublic))
			{
				SecurityUtils.DemandReflectionAccess(declaringType);
			}
			return ctor.Invoke(args);
		}

		[SecuritySafeCritical]
		private static void DemandGrantSet(Assembly assembly)
		{
#if !NETSTANDARD
			PermissionSet permissionSet = assembly.PermissionSet;
			permissionSet.AddPermission(SecurityUtils.RestrictedMemberAccessPermission);
			permissionSet.Demand();
#endif
		}

		private static void DemandReflectionAccess(Type type)
		{
			try
			{
				SecurityUtils.MemberAccessPermission.Demand();
			}
			catch (SecurityException securityException)
			{
				SecurityUtils.DemandGrantSet(type.Assembly);
			}
		}

		internal static object FieldInfoGetValue(FieldInfo field, object target)
		{
			Type declaringType = field.DeclaringType;
			if (declaringType == null)
			{
				if (!field.IsPublic)
				{
					SecurityUtils.DemandGrantSet(field.Module.Assembly);
				}
			}
			else if (!(declaringType != null) || !declaringType.IsVisible || !field.IsPublic)
			{
				SecurityUtils.DemandReflectionAccess(declaringType);
			}
			return field.GetValue(target);
		}

		private static bool GenericArgumentsAreVisible(MethodInfo method)
		{
			if (method.IsGenericMethod)
			{
				Type[] genericArguments = method.GetGenericArguments();
				for (int i = 0; i < (int)genericArguments.Length; i++)
				{
					if (!genericArguments[i].IsVisible)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool HasReflectionPermission(Type type)
		{
			bool flag;
			try
			{
				SecurityUtils.DemandReflectionAccess(type);
				flag = true;
			}
			catch (SecurityException securityException)
			{
				return false;
			}
			return flag;
		}

		internal static object MethodInfoInvoke(MethodInfo method, object target, object[] args)
		{
			Type declaringType = method.DeclaringType;
			if (declaringType == null)
			{
				if (!method.IsPublic || !SecurityUtils.GenericArgumentsAreVisible(method))
				{
					SecurityUtils.DemandGrantSet(method.Module.Assembly);
				}
			}
			else if (!declaringType.IsVisible || !method.IsPublic || !SecurityUtils.GenericArgumentsAreVisible(method))
			{
				SecurityUtils.DemandReflectionAccess(declaringType);
			}
			return method.Invoke(target, args);
		}

		internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic)
		{
			return SecurityUtils.SecureConstructorInvoke(type, argTypes, args, allowNonPublic, BindingFlags.Default);
		}

		internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic, BindingFlags extraFlags)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsVisible)
			{
				SecurityUtils.DemandReflectionAccess(type);
			}
			else if (allowNonPublic && !SecurityUtils.HasReflectionPermission(type))
			{
				allowNonPublic = false;
			}
			BindingFlags bindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | extraFlags;
			if (!allowNonPublic)
			{
				bindingFlag = bindingFlag & (BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.ExactBinding | BindingFlags.SuppressChangeType | BindingFlags.OptionalParamBinding | BindingFlags.IgnoreReturn);
			}
			ConstructorInfo constructor = type.GetConstructor(bindingFlag, null, argTypes, null);
			if (constructor == null)
			{
				return null;
			}
			return constructor.Invoke(args);
		}

		internal static object SecureCreateInstance(Type type)
		{
			return SecurityUtils.SecureCreateInstance(type, null, false);
		}

		internal static object SecureCreateInstance(Type type, object[] args, bool allowNonPublic)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			BindingFlags bindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;
			if (!type.IsVisible)
			{
				SecurityUtils.DemandReflectionAccess(type);
			}
			else if (allowNonPublic && !SecurityUtils.HasReflectionPermission(type))
			{
				allowNonPublic = false;
			}
			if (allowNonPublic)
			{
				bindingFlag |= BindingFlags.NonPublic;
			}
			return Activator.CreateInstance(type, bindingFlag, null, args, null);
		}

		internal static object SecureCreateInstance(Type type, object[] args)
		{
			return SecurityUtils.SecureCreateInstance(type, args, false);
		}
	}
}