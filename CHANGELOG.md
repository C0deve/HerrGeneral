# Changelog

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
