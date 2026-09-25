# Changelog

## [1.4.0](https://github.com/C0deve/HerrGeneral/compare/v1.3.0...v1.4.0) (2026-09-25)


### Features

* add command concurrency ([8abec88](https://github.com/C0deve/HerrGeneral/commit/8abec88d7b03c7f609374bb75ca973cb4ace2ec8))
* add post-transaction and side-effect handlers with pipeline isolation ([79e0095](https://github.com/C0deve/HerrGeneral/commit/79e00957158e879a59c60cd5bf67d4339f3078e4))
* **core:** introduce `CommandConcurrencyLimiter` and enhance cancellation/DI handling in Mediator ([15ed851](https://github.com/C0deve/HerrGeneral/commit/15ed851c8932e5ca6cd5ad647991247e0706b02b))
* **opentelemetry:** add HerrGeneral.OpenTelemetry package with instrumentation extensions and tests ([adb77e8](https://github.com/C0deve/HerrGeneral/commit/adb77e8f2d4b8bf51d74df6ddebe36d6d8d05c58))
* **solution:** add solution file with project structure ([bfa7969](https://github.com/C0deve/HerrGeneral/commit/bfa7969c08ce6c69970679c1ac537190cc131370))
* **telemetry:** introduce OpenTelemetry tracing, metrics and activity tree formatting ([864bdc2](https://github.com/C0deve/HerrGeneral/commit/864bdc2edd57b39ea44e9be1ed7fa122766df631))


### Bug Fixes

* add junie to gitignore ([6937473](https://github.com/C0deve/HerrGeneral/commit/6937473ce0079074a98f78a07b7ab759edbc429e))
* correct semaphore initialization, allow multi-handler classes, and secure DI registration ([fca5d15](https://github.com/C0deve/HerrGeneral/commit/fca5d1559ae8b98c08e07b79b452fe5dc17fd437))

## [1.5.0] (2026-09-23)

### Features

* **opentelemetry:** introduced native distributed tracing and metrics via standard .NET BCL `System.Diagnostics.ActivitySource` and `System.Diagnostics.Metrics.Meter` (`HerrGeneral`):
  * Added package `HerrGeneral.OpenTelemetry` with `TracerProviderBuilder` and `MeterProviderBuilder` extension methods (`AddHerrGeneralInstrumentation`).
  * Command root activities (`HerrGeneral.ExecuteCommand`) with semantic tags, error status, and exception events.
  * Child activities for each pipeline phase: `HerrGeneral.WriteSide.Dispatch`, `HerrGeneral.WriteSide.HandleEvent`, `HerrGeneral.UnitOfWork`, `HerrGeneral.SyncProjections.Dispatch`, `HerrGeneral.SyncProjections.HandleEvent`, `HerrGeneral.PostTransaction.Dispatch`, `HerrGeneral.PostTransaction.HandleEvent`, `HerrGeneral.ReadSide.Dispatch`, `HerrGeneral.ReadSide.HandleEvent`.
  * Standard metrics: `herrgeneral.commands.total`, `herrgeneral.commands.duration`, `herrgeneral.events.total`, `herrgeneral.events.duration`, `herrgeneral.commands.active`.
* **tracing:** replaced `CommandExecutionTracer` with `ActivityTreeCollector` and `ActivityTreeFormatter` providing an identical hierarchical ASCII tree log representation.
* **compatibility:** maintained 100% backward compatibility with existing configuration options (`EnableCommandExecutionTracing`).

## [1.4.0] (2026-09-22)

### Features

* **handlers:** introduced unified `IHandle...` handler taxonomy:
  * `IHandleCrossAggregate<in TEvent, TAggregate>` for in-transaction aggregate modifications and cascading events.
  * `IHandleSyncProjection<in TEvent>` for in-transaction synchronous projections and outbox records (rolls back transaction on error).
  * `IHandlePostProjection<in TEvent>` for post-transaction eventual consistency read models (post-commit execution with error isolation).
  * `IHandleSideEffect<in TEvent>` for post-transaction external side effects, emails, webhooks, and message publishing.
* **pipeline:** restructured command execution pipeline to strictly enforce transactional boundaries around `IUnitOfWork`:
  * `WithTransactionalProjectionDispatching` inside transaction before `Commit()`.
  * `WithPostTransactionDispatching` outside transaction after `Commit()`.
* **tracing:** added granular execution tracing for sync projections, post-transaction projections, and side effects in `CommandExecutionTracer`.
* **configuration:** added fluent scanning methods `ScanSideEffectsOn`, `ScanSyncProjectionsOn`, `ScanPostProjectionsOn` in `ConfigurationBuilder`.
* **compatibility:** maintained full backward compatibility with `ICrossAggregateChangeHandler` and `IProjectionEventHandler`.

## [1.3.0](https://github.com/C0deve/HerrGeneral/compare/v1.2.0...v1.3.0) (2025-12-01)


### Features

* **start:** first release ([db75cea](https://github.com/C0deve/HerrGeneral/commit/db75cea8545b2c36ed8ef9c2fde6bb4e3f75bfe0))

## [1.2.0](https://github.com/C0deve/HerrGeneral/compare/v1.1.0...v1.2.0) (2025-11-30)


### Features

* **start:** first release ([b5ef054](https://github.com/C0deve/HerrGeneral/commit/b5ef054d870694c17d40cf58a4e62fd89d6fc408))

## [1.1.0](https://github.com/C0deve/HerrGeneral/compare/v1.0.0...v1.1.0) (2025-11-21)


### Features

* **start:** first conventional commit to get a release workflow update ([37eedf4](https://github.com/C0deve/HerrGeneral/commit/37eedf49a97d61479297dbbf08353796d66d844e))
