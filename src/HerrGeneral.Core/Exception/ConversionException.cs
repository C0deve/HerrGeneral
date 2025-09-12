using HerrGeneral.Core;

namespace HerrGeneral.Exception;

/// <summary>
/// The conversion of the external handler result to <see cref="Result{TResult}"/> failed 
/// </summary>
public class ConversionException(Type resultType, Type declaringType, System.Exception innerException)
    : System.Exception($"Conversion of {resultType.GetFriendlyName()} failed. See mapping definition for {declaringType.GetFriendlyName()}", innerException);