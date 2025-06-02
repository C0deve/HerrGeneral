using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Error;

/// <summary>
/// The conversion of the external handler result to <see cref="Result{TResult}"/> failed 
/// </summary>
public class ConversionException(Type resultType, Type declaringType, Exception innerException)
    : Exception($"Conversion of {resultType.GetFriendlyName()} failed. See mapping definition for {declaringType.GetFriendlyName()}", innerException);