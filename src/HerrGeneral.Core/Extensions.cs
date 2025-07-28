namespace HerrGeneral.Core;

/// <summary>
/// Extensions methods
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Fluent guid validation
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    internal static bool IsEmpty(this Guid guid) => guid == Guid.Empty;

    
    /// <summary>
    /// Display the type with a friendly name
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static string GetFriendlyName(this Type type)
    {
        var friendlyName = type.Name;
        if (!type.IsGenericType) return friendlyName;

        var iBacktick = friendlyName.IndexOf('`');

        if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);

        friendlyName += "<";
        var typeParameters = type.GetGenericArguments();
        for (var i = 0; i < typeParameters.Length; ++i)
        {
            var typeParamName = GetFriendlyName(typeParameters[i]);
            friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
        }

        friendlyName += ">";

        return friendlyName;
    }

    /// <summary>
    /// Sends a command through the mediator using a fluent syntax.
    /// This extension method improves readability by allowing direct Send() calls on command objects.
    /// </summary>
    /// <param name="command">The command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <returns>A task containing the result of the command execution</returns>
    /// <example>
    /// <code>
    /// await new CreatePerson("John", "Doe").SendFromMediator(mediator);
    /// </code>
    /// </example>
    public static Task<Result> SendFrom(this object command, Mediator mediator) => mediator.Send(command);

    /// <summary>
    /// Sends a command through the mediator using a fluent syntax and returns a typed value.
    /// This extension method improves readability by allowing direct Send() calls on command objects
    /// while specifying the expected return type.
    /// </summary>
    /// <param name="command">The command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TValue">The type of the value returned by the command (e.g., Guid for an identifier)</typeparam>
    /// <returns>A task containing the result with a value of type <typeparamref name="TValue"/></returns>
    /// <example>
    /// <code>
    /// Guid personId = await new CreatePerson("John", "Doe").SendFromMediator&lt;Guid&gt;(mediator);
    /// </code>
    /// </example>
    public static Task<Result<TValue>> SendFrom<TValue>(this object command, Mediator mediator) => mediator.Send<TValue>(command);
    
}