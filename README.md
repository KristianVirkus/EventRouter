# Event Routing

The goal of this library is to provide a generic one-way event routing mechanism for any kind of events.

## Principles

The main component of this library is the `Hub<T>` class which implements `IHub<T>` and handles routing of events via configured routers.

The type of routed events is specified as the generic type parameter `T` to the class `Hub<T>` and must implement `IRoutable`.

The routers forwarding events of type `IRoutable` themselves must implement `IRouter<T>` with `T` being the type of events.

Routers can be assigned to a hub by calling a hub's `ReconfigureAsync` method handing in an instance of `HubConfiguration<T>` which contains references to the router instances.

A configuration can also specify preprocessors implementing `IRoutablePreprocessor<T>`. Preprocessors are automatically invoked for each single forwarded event by the hub right before handing the event over to the configured routers allowing for filtering or replacing of events depending on the preprocessors' logic. Any preprocessor may decide via the `OnEnqueueing` property whether the preprocessing needs to be done immediately while enqueueing (in a blocking fashion) or asynchronously right before forwarding an event to the configured routers.

A built-in preprocessor is `FilterPreprocessor` which allows for simple filtering based on allowing and blocking conditions. In order to have an event pass the filter, all allowing conditions must be successfully tested while none of the blocking conditions may apply.

Instead of creating a `HubConfiguration<T>` instance directly, the builder `HubConfigurationBuilder<T>` can also be used to fluently define a hub configuration and finally calling the `Build()` method to create a related `HubConfiguration<T>` instance.

## Example

The example app `EventRouter.SampleApp` defines an event `MessageEvent` containing a printable `Message`, a router `ConsoleRouter` which prints forwarded events on the console, and utilizes a `Hub` configured to forward all events whose messages start with the text `Important`.

## Known Issues

- After calling `ReconfigureAsync(null, ?)` the previously configured queue capacity is still in order.

## License

This library is licensed under the MIT license. See `LICENSE` in the root directory of this repository.
