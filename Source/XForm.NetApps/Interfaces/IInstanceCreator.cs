// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.Core.Interfaces;

public interface IInstanceCreator
{
	/// <summary>
	/// Instantiate a type from the provided assembly path and type name.
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	/// <param name="type"></param>
	/// <param name="constructorParameters"></param>
	/// <returns></returns>
	TType InstantiateType<TType>(string type, params object[] constructorParameters) where TType : class;
}
