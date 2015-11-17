using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// An Active Directory group, which is a collection of its <see cref="ActiveDirectoryGroupMember"/> objects.
    /// </summary>
    public class ActiveDirectoryGroup : Collection<ActiveDirectoryGroupMember>
    {
    }
}