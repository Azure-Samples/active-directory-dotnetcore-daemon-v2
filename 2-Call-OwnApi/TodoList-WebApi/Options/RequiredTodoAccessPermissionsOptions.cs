using System.Collections.Generic;

namespace TodoList_WebApi.Options;

public class RequiredTodoAccessPermissionsOptions
{
    public const string RequiredTodoAccessPermissions = "RequiredTodoAccessPermissions";

    // Set these keys as constants to make them accessible to the 'RequiredScopeOrAppPermission' attribute. You can add
    // multiple spaces separated entries for each string in the 'appsettings.json' file and they will be used by the
    // 'RequiredScopeOrAppPermission' attribute.
    public const string RequiredDelegatedTodoReadClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredDelegatedTodoReadClaims";

    public const string RequiredDelegatedTodoWriteClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredDelegatedTodoWriteClaims";

    public const string RequiredApplicationTodoReadClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredApplicationTodoReadClaims";

    public const string RequiredApplicationTodoReadWriteClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredApplicationTodoReadWriteClaims";

    /// <summary>
    /// Holds a space separated string containing all delegated claims required to read to-do's.
    /// </summary>
    public string RequiredDelegatedTodoReadClaims { get; set; }

    /// <summary>
    /// Holds a space separated string containing all delegated claims required to write to-do's.
    /// </summary>
    public string RequiredDelegatedTodoWriteClaims { get; set; }

    /// <summary>
    /// Holds a space separated string containing all application claims required to write to-do's.
    /// </summary>
    public string RequiredApplicationTodoReadWriteClaims { get; set; }
}