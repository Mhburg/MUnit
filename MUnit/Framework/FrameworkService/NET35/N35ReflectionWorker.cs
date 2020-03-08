// <copyright file="N35ReflectionWorker.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if NET35

using MUnit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MUnit.Framework.NET35
{
    // TODO Add Reflection only code for attribute.

    /// <summary>
    /// Reflection helper for .Net Framework 3.5.
    /// </summary>
    internal class N35ReflectionWorker : IFrameworkRelfection
    {
        /*********************************************************************************************************

           Inheritance chain Object <-- MemberInfo <-- Type, EventInfo, FieldInfo, PropertyInfo, MethodBase, etc..
           https://docs.microsoft.com/en-us/dotnet/api/system.reflection.memberinfo?view=netframework-3.5

        *********************************************************************************************************/
        private readonly bool _reflectionOnly;

        private readonly Dictionary<MemberInfo, Dictionary<Type, List<Attribute>>> _attributeCache =
            new Dictionary<MemberInfo, Dictionary<Type, List<Attribute>>>();

        private readonly Dictionary<MemberInfo, Dictionary<Type, List<Attribute>>> _inheritedAttributeCache =
            new Dictionary<MemberInfo, Dictionary<Type, List<Attribute>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="N35ReflectionWorker"/> class.
        /// </summary>
        /// <param name="reflectionOnly">If it is in reflection-only context.</param>
        public N35ReflectionWorker(bool reflectionOnly)
        {
            _reflectionOnly = reflectionOnly;
        }

        /// <inheritdoc/>
        public virtual bool IsAttributeDefined(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            ThrowUtilities.NullArgument(memberInfo, nameof(memberInfo));
            ThrowUtilities.NullArgument(attributeType, nameof(attributeType));

            return GetAttributes(memberInfo, inherit).ContainsKey(attributeType);
        }

        /// <inheritdoc/>
        public virtual bool HasAttributeDerivedFrom(MemberInfo memberInfo, Type baseAttributeType, bool inherit)
        {
            ThrowUtilities.NullArgument(memberInfo, nameof(memberInfo));
            ThrowUtilities.NullArgument(baseAttributeType, nameof(baseAttributeType));

            return GetAttributes(memberInfo, inherit)
                        .Keys
                        .Any(t => t.IsSubclassOf(baseAttributeType));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Attribute> GetCustomAttributes(MemberInfo memberInfo, Type type, bool inherit)
        {
            if (GetAttributes(memberInfo, inherit).TryGetValue(type, out List<Attribute> attributes))
            {
                return attributes;
            }

            return null;
        }

        /// <inheritdoc/>
        public virtual Attribute GetCustomAttribute(MemberInfo memberInfo, Type type, bool inherit)
        {
            return GetCustomAttributes(memberInfo, type, inherit)?.First();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Attribute> GetDerivedAttributes(MemberInfo memberInfo, Type attributeBaseType, bool inherit)
        {
            return GetAttributes(memberInfo, inherit)
                .Where(pair => pair.Key.GetType().IsSubclassOf(attributeBaseType))
                .SelectMany(pair => pair.Value);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Attribute> GetAttributesHaveBase(MemberInfo memberInfo, Type attributeBaseType, bool inherit)
        {
            List<Attribute> allAttributes = new List<Attribute>();
            IEnumerable<Attribute> baseAttributes = GetCustomAttributes(memberInfo, attributeBaseType, inherit);
            IEnumerable<Attribute> derivedAttributes = GetDerivedAttributes(memberInfo, attributeBaseType, inherit);

            if (baseAttributes != null)
                allAttributes.AddRange(baseAttributes);

            if (derivedAttributes != null)
                allAttributes.AddRange(derivedAttributes);

            return allAttributes.Any() ? allAttributes : null;
        }

        /// <inheritdoc/>
        public virtual Dictionary<Type, List<Attribute>> GetAttributes(MemberInfo memberInfo, bool inherit)
        {
            if (inherit)
            {
                lock (_inheritedAttributeCache)
                {
                    if (!_inheritedAttributeCache.ContainsKey(memberInfo))
                    {
                        PopulateCache(_inheritedAttributeCache, memberInfo, inherit);
                    }

                    return _inheritedAttributeCache[memberInfo];
                }
            }
            else
            {
                lock (_attributeCache)
                {
                    if (!_attributeCache.ContainsKey(memberInfo))
                    {
                        PopulateCache(_attributeCache, memberInfo, inherit);
                    }

                    return _attributeCache[memberInfo];
                }
            }
        }

        /// <inheritdoc/>
        public virtual Attribute GetAttributeAssignableTo(MemberInfo memberInfo, Type assignToType, bool inherit)
        {
            return this.GetAttributes(memberInfo, inherit)
                .Where(pair => assignToType.IsAssignableFrom(pair.Key))
                .SelectMany(pair => pair.Value)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<MethodInfo> GetMethods(Type type)
        {
            return type.GetMethods().AsEnumerable();
        }

        /// <inheritdoc/>
        public virtual string GetAssemblyFullPath(Type type)
        {
            return type.Assembly.Location;
        }

        /// <inheritdoc/>
        public virtual string GetNamespace(Type type)
        {
            return type.Namespace;
        }

        /// <inheritdoc/>
        public virtual string GetTypeFullName(Type type)
        {
            return type.FullName;
        }

        /// <inheritdoc/>
        public virtual string GetMethodName(MethodInfo method)
        {
            return method.Name;
        }

        /// <inheritdoc/>
        public virtual string GetMethodFullName(MethodInfo method)
        {
            return string.Concat(this.GetTypeFullName(method.DeclaringType), ".", this.GetMethodName(method));
        }

        /// <inheritdoc/>
        public Assembly GetAssembly(Type type)
        {
            return type.Assembly;
        }

        private void PopulateCache(Dictionary<MemberInfo, Dictionary<Type, List<Attribute>>> cache, MemberInfo memberInfo, bool inherit)
        {
            IEnumerable<Attribute> attrs = memberInfo.GetCustomAttributes(inherit).OfType<Attribute>();
            Dictionary<Type, List<Attribute>> attributeCache = new Dictionary<Type, List<Attribute>>();

            // There could be duplicate attributes.
            foreach (Attribute attr in attrs)
            {
                Type attributeType = attr.GetType();
                if (!attributeCache.ContainsKey(attributeType))
                {
                    attributeCache[attributeType] = new List<Attribute>() { attr };
                }
                else
                {
                    attributeCache[attributeType].Add(attr);
                }
            }

            cache[memberInfo] = attributeCache;
        }
    }
}

#endif