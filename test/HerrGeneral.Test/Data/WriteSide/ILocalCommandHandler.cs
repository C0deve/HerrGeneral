﻿using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public interface ILocalCommandHandler<in TCommand, TResult> where TCommand : CommandBase
{
    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    MyResult<TResult> Handle(TCommand command);
}

public interface ILocalCommandHandler<in TCommand> : ILocalCommandHandler<TCommand, Unit> where TCommand : CommandBase;