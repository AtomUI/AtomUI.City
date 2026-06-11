# AtomUI.City

[![CI](https://github.com/AtomUI/AtomUI.City/actions/workflows/ci.yml/badge.svg)](https://github.com/AtomUI/AtomUI.City/actions/workflows/ci.yml)
[![License: LGPL-3.0-only](https://img.shields.io/badge/license-LGPL--3.0--only-blue.svg)](LICENSE)

AtomUI.City is a full-stack application framework for building modular desktop business applications on top of AtomUI and Avalonia.

It provides an opinionated application model for the AtomUI/Avalonia ecosystem: hosting, lifecycle, modularity, MVVM, routing, state management, data access, security, localization, plugin infrastructure, build integration, templates, CLI workflows, and testing utilities.

> Status: early development. Public APIs are still evolving toward the first framework release.

## Architecture Vision

AtomUI.City sits above AtomUI/Avalonia as the application framework layer.

AtomUI and Avalonia provide controls, styling, theming, rendering, and low-level UI behavior. AtomUI.City focuses on the application architecture around them: how an application starts, how modules are composed, how routes create ViewModels, how ViewModels become views, how state and events flow, how plugins are loaded, and how the whole system is built, tested, and shipped.

The framework is intentionally business-agnostic. It does not define workbench, document, dashboard, domain model, repository, or application service patterns as built-in concepts. Applications can add those layers when needed, but AtomUI.City itself provides reusable infrastructure.

## Programming Model

AtomUI.City uses MVVM as its primary application programming model.

The default flow is:

```text
Route
-> ViewModel Target
-> ViewModel
-> View
-> Outlet
-> Visual Tree
```

Routing is responsible for resolving the route target. MVVM is responsible for ViewModel activation, commands, interactions, and validation contracts. Presentation is responsible for ViewModel-to-View resolution, outlet commits, UI dispatcher integration, and the bridge to AtomUI/Avalonia.

This keeps the responsibilities explicit:

- Routing decides where the application goes.
- MVVM defines how application interaction is modeled.
- Presentation decides how ViewModels become UI.
- AtomUI/Avalonia provide the actual UI foundation.

## Lifecycle-First Runtime

Lifecycle is a first-class concept in AtomUI.City.

The framework models lifecycle across the full desktop application runtime:

- Application startup, running, suspend, resume, and shutdown.
- Module discovery, configuration, initialization, and shutdown.
- Route enter, leave, guard, resolver, and navigation transactions.
- ViewModel activation, deactivation, and disposal.
- State subscriptions, reactions, snapshots, and cleanup.
- EventBus subscriptions and scoped event delivery.
- Plugin install, load, enable, disable, unload, and rollback.

The goal is to make long-running desktop applications predictable: subscriptions are scoped, resources are released, plugin boundaries are explicit, and UI-thread work is controlled.

## Modular Foundation

Modules are the basic unit of framework composition.

A module can contribute services, configuration, routes, permissions, localization resources, data clients, event handlers, presentation resources, and plugin extension points. Module dependencies are declared explicitly and are intended to be processed at build time whenever possible.

The first version keeps core runtime concepts in `AtomUI.City.Core` instead of splitting the kernel too aggressively. Hosting, lifecycle, modularity, configuration, dependency injection, diagnostics, and threading primitives belong to the core package.

## Package Model

AtomUI.City is organized as a focused package family:

| Package | Responsibility |
|---|---|
| `AtomUI.City.Core` | Host, lifecycle, modularity, DI, configuration, diagnostics, threading. |
| `AtomUI.City.Mvvm` | ViewModel activation, commands, interactions, validation. |
| `AtomUI.City.State` | Injectable state, scoped state, computed state, snapshots, subscriptions. |
| `AtomUI.City.Routing` | Route definitions, matching, navigation, guards, ViewModel targets. |
| `AtomUI.City.Presentation` | View location, route outlets, dispatcher bridge, UI runtime integration. |
| `AtomUI.City.Data` | HTTP, gRPC, SignalR, request pipeline, connection lifecycle. |
| `AtomUI.City.Security` | Authentication state, permissions, authorization policies. |
| `AtomUI.City.EventBus` | Typed event bus, scoped subscriptions, dispatch policies. |
| `AtomUI.City.Localization` | Culture switching, lazy language packages, assembly-based resources. |
| `AtomUI.City.PluginSystem` | Plugin metadata, package layout, loading, unloading, capabilities. |
| `AtomUI.City.Build` | MSBuild integration, manifests, packaging, output conventions. |
| `AtomUI.City.Generators` | Source generators and analyzers for AOT-friendly manifests. |
| `AtomUI.City.Cli` | `atomui city ...` developer workflows. |
| `AtomUI.City.Templates` | Application, module, page, plugin, localization, and test templates. |
| `AtomUI.City.Testing` | Test hosts, fake dispatchers, lifecycle drivers, framework test utilities. |

## AOT-First Design

AtomUI.City is designed to be friendly to Native AOT, trimming, and predictable startup.

The framework prefers:

- Explicit contracts over convention-only runtime discovery.
- Source generators over reflection scanning.
- Strongly typed descriptors over dynamic metadata.
- Build-time manifests over runtime assembly traversal.
- Analyzer diagnostics over late runtime failures.

Runtime reflection may exist for controlled compatibility paths, but it is not the default design center.

## Plugin-Aware Architecture

The plugin system is part of the framework architecture, not an afterthought.

Plugins can contribute modules and framework capabilities at runtime. A plugin should be packageable as an independent NuGet package and loadable by the host application through explicit metadata, dependency validation, capability checks, lifecycle coordination, and unload rules.

Plugin boundaries are designed to interact with Host, Module, Routing, EventBus, Security, Localization, Presentation, and State without allowing plugins to bypass the host runtime.

## CLI

The CLI entry point is:

```bash
atomui city <command>
```

The CLI is designed for both human developers and AI-assisted workflows. Commands are expected to support structured output, non-interactive execution, diagnostics, project inspection, and repeatable build/test/template operations.

## License

AtomUI.City is licensed under the GNU Lesser General Public License v3.0 only.

See [LICENSE](LICENSE).
