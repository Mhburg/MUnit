// <copyright file="IFrameworkRelfection.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MUnit.Framework
{
    /// <summary>
    /// Reflection worker that should be implemented individually for different runtime.
    /// </summary>
    public interface IFrameworkRelfection
    {
        /// <summary>
        /// Get the Attributes (TypeName/TypeObject) for a given member.
        /// </summary>
        /// <param name="memberInfo">The member to inspect.</param>
        /// <param name="inherit">Look at inheritance chain.</param>
        /// <returns>attributes defined.</returns>
        Dictionary<Type, List<Attribute>> GetAttributes(MemberInfo memberInfo, bool inherit);

        /// <summary>
        /// Get attributes that are or are derived from <paramref name="attributeBaseType"/> on <paramref name="memberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">Member to be reflected on.</param>
        /// <param name="attributeBaseType">Attribute base type.</param>
        /// <param name="inherit">Look at inheritance chain.</param>
        /// <returns>attributes that are or are derived from <paramref name="attributeBaseType"/> on <paramref name="memberInfo"/>.</returns>
        IEnumerable<Attribute> GetAttributesHaveBase(MemberInfo memberInfo, Type attributeBaseType, bool inherit);

        /// <summary>
        /// Returns the attribute of the specified type. Null if no attribute of the specified type is defined.
        /// </summary>
        /// <param name="memberInfo">The method to inspect.</param>
        /// <param name="type">The attribute type.</param>
        /// <param name="inherit">whether to search base classes or not.</param>
        /// <returns>Attribute of the specified type. Null if not found.</returns>
        Attribute GetCustomAttribute(MemberInfo memberInfo, Type type, bool inherit);

        /// <summary>
        /// Get instances of an attribute type on a member.
        /// </summary>
        /// <param name="memberInfo">Memeber for which attributes needs to be retrieved.</param>
        /// <param name="type">Type of attribute to retrieve.</param>
        /// <param name="inherit">whether to search base classes or not.</param>
        /// <returns>Instances of an attribute type or null if none is found.</returns>
        IEnumerable<Attribute> GetCustomAttributes(MemberInfo memberInfo, Type type, bool inherit);

        /// <summary>
        /// Get attributes defined on a method which are derived from <paramref name="attributeBaseType"/>.
        /// </summary>
        /// <param name="memberInfo">The member to inspect.</param>
        /// <param name="attributeBaseType">Base type of returned attributes.</param>
        /// <param name="inherit">Look at inheritance chain.</param>
        /// <returns>An instance of the attribute.</returns>
        IEnumerable<Attribute> GetDerivedAttributes(MemberInfo memberInfo, Type attributeBaseType, bool inherit);

        /// <summary>
        /// Returns true when specified class/member has attribute derived from specific attribute.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="baseAttributeType">The base attribute type.</param>
        /// <param name="inherit">Should look at inheritance tree.</param>
        /// <returns>An object derived from Attribute that corresponds to the instance of found attribute.</returns>
        bool HasAttributeDerivedFrom(MemberInfo memberInfo, Type baseAttributeType, bool inherit);

        /// <summary>
        /// Checks to see if the parameter memberInfo contains the parameter attribute or not.
        /// </summary>
        /// <param name="memberInfo">Member/Type to test.</param>
        /// <param name="attributeType">Attribute to search for.</param>
        /// <param name="inherit">Look throug bases of <paramref name="memberInfo"/> or not.</param>
        /// <returns>True if the attribute of the specified type is defined.</returns>
        bool IsAttributeDefined(MemberInfo memberInfo, Type attributeType, bool inherit);

        /// <summary>
        /// Get attribute that can assign to <paramref name="assignToType"/>.
        /// </summary>
        /// <param name="memberInfo"> Object to be reflected on. </param>
        /// <param name="assignToType"> Type that attribute can be assigned to. </param>
        /// <param name="inherit">Look throug bases of <paramref name="memberInfo"/> or not.</param>
        /// <returns> Attribute that can assign to <paramref name="assignToType"/>. </returns>
        Attribute GetAttributeAssignableTo(MemberInfo memberInfo, Type assignToType, bool inherit);

        /// <summary>
        /// Get methods from Type.
        /// </summary>
        /// <param name="type">Type to get methods from.</param>
        /// <returns>Methods in the type.</returns>
        IEnumerable<MethodInfo> GetMethods(Type type);

        /// <summary>
        /// Get full path to the assembly that contains <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> Type to be reflected on.</param>
        /// <returns> Full path to Assembly. </returns>
        string GetAssemblyFullPath(Type type);

        /// <summary>
        /// Get the namespace of <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> Type to be reflected on. </param>
        /// <returns> Name of the namespace. </returns>
        string GetNamespace(Type type);

        /// <summary>
        /// Get the fullname of <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> Type to be reflected on. </param>
        /// <returns> Full name of <paramref name="type"/>. </returns>
        string GetTypeFullName(Type type);

        /// <summary>
        /// Gets assembly from type.
        /// </summary>
        /// <param name="type"> Type to be reflected on. </param>
        /// <returns> Assembly that contains this type. </returns>
        Assembly GetAssembly(Type type);

        /// <summary>
        /// Get name from method.
        /// </summary>
        /// <param name="method"> Method to be reflected on. </param>
        /// <returns> Name of the method. </returns>
        string GetMethodName(MethodInfo method);

        /// <summary>
        /// Get fully qualified name for method.
        /// </summary>
        /// <param name="method"> Mehotd to be reflected on. </param>
        /// <returns> Full name of this method. </returns>
        string GetMethodFullName(MethodInfo method);
    }
}