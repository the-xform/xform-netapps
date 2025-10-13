// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Reflection;

namespace XForm.NetApps;

public class AssemblyNameComparer : IEqualityComparer<AssemblyName>
{
    public bool Equals(AssemblyName? x, AssemblyName? y)
    {
        if(ReferenceEquals(x,y)) return true;
        if(ReferenceEquals(y,null)) return false;
        if(ReferenceEquals (x,null)) return false;
        return x.FullName.Equals(y.FullName, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(AssemblyName obj)
    {
        return obj.FullName.GetHashCode();
    }
}
