# Rochas.RESTClient

[English](#english) | [Português](#português) | [Español](#español) | [Français](#français) | [Deutsch](#deutsch)

---

## English

**Rochas.RESTClient** is a lightweight generic and resilient RESTful client for .NET. It provides async and sync wrappers around `HttpClient` with built-in retry logic, timeout support, and custom header management — ideal for calling REST APIs from services, workers, or background jobs.

### Features

- Generic REST operations: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` (async and sync).
- Resilience queue with configurable retry count and delay between retries.
- Per-request timeout support (in seconds).
- Custom HTTP headers on every call.
- Query string parameter encoding via `GetWithParams` / `DeleteWithParams`.
- Typed response variants: `PostWithResponse<R>`, `PutWithResponse<R>`, `PatchWithResponse<R>`.
- `ILogger` integration for resilience diagnostics and error logging.
- Targets `netstandard2.1` — runs on .NET Core, .NET 5+ and later frameworks.

### Installation

```bash
dotnet add package Rochas.RESTClient
```

### Quick Start

#### Simple GET/POST

```csharp
using Rochas.Net.Connectivity;

var client = new RESTClient<MyDto>();

// GET
var item = await client.Get("https://api.example.com/items/42");

// POST
var ok = await client.Post("https://api.example.com/items", new MyDto { Name = "test" });
```

#### With resilience (retries)

```csharp
using Rochas.Net.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

// 3 retries, 500 ms delay between attempts
var client = new RESTClient<OrderDto>(NullLogger<OrderDto>.Instance, callRetries: 3, retriesDelay: 500);

await client.Post("https://api.example.com/orders", new OrderDto { Id = 1 });
```

#### Sync (worker-friendly)

```csharp
var item = client.GetSync("https://api.example.com/items/42");
var ok = client.PostSync("https://api.example.com/items", new MyDto { Name = "test" });
```

### Tests

![tests](https://img.shields.io/badge/tests-52-blue)
![passing](https://img.shields.io/badge/passing-31-brightgreen)

**52 unit tests** (xUnit) — **31 passing**. Pre-existing timeout-based failures in resilience tests (environment-dependent timing thresholds).

### License

GPL v2 — see `GNUv2_License.txt`.

---

## Português

**Rochas.RESTClient** é um cliente RESTful genérico, leve e resiliente para .NET. Fornece wrappers síncronos e assíncronos ao redor do `HttpClient` com lógica de retry integrada, suporte a timeout e gerenciamento de headers customizados — ideal para chamar APIs REST a partir de serviços, workers ou jobs em background.

### Funcionalidades

- Operações REST genéricas: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` (assíncrono e síncrono).
- Fila de resiliência com contagem de retries configurável e atraso entre tentativas.
- Suporte a timeout por requisição (em segundos).
- Headers HTTP customizados em cada chamada.
- Codificação de parâmetros de query string via `GetWithParams` / `DeleteWithParams`.
- Variantes de resposta tipadas: `PostWithResponse<R>`, `PutWithResponse<R>`, `PatchWithResponse<R>`.
- Integração com `ILogger` para diagnósticos de resiliência e registro de erros.
- Compatível com `netstandard2.1` — roda no .NET Core, .NET 5+ e versões posteriores.

### Instalação

```bash
dotnet add package Rochas.RESTClient
```

### Início rápido

#### GET/POST simples

```csharp
using Rochas.Net.Connectivity;

var client = new RESTClient<MyDto>();

// GET
var item = await client.Get("https://api.example.com/items/42");

// POST
var ok = await client.Post("https://api.example.com/items", new MyDto { Name = "test" });
```

#### Com resiliência (retries)

```csharp
using Rochas.Net.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

// 3 retries, 500 ms de atraso entre tentativas
var client = new RESTClient<OrderDto>(NullLogger<OrderDto>.Instance, callRetries: 3, retriesDelay: 500);

await client.Post("https://api.example.com/orders", new OrderDto { Id = 1 });
```

#### Síncrono (worker-friendly)

```csharp
var item = client.GetSync("https://api.example.com/items/42");
var ok = client.PostSync("https://api.example.com/items", new MyDto { Name = "test" });
```

### Testes

![tests](https://img.shields.io/badge/tests-52-blue)
![passing](https://img.shields.io/badge/passing-31-brightgreen)

**52 testes unitários** (xUnit) — **31 passando**. Falhas pré-existentes baseadas em timeout nos testes de resiliência (limites de tempo dependentes do ambiente).

### Licença

GPL v2 — veja `GNUv2_License.txt`.

---

## Español

**Rochas.RESTClient** es un cliente RESTful genérico, ligero y resiliente para .NET. Proporciona wrappers asíncronos y síncronos alrededor de `HttpClient` con lógica de reintentos integrada, soporte de timeout y gestión de headers personalizados — ideal para llamar APIs REST desde servicios, workers o jobs en segundo plano.

### Características

- Operaciones REST genéricas: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` (asíncrono y síncrono).
- Cola de resiliencia con conteo de reintentos configurable y retardo entre intentos.
- Soporte de timeout por petición (en segundos).
- Headers HTTP personalizados en cada llamada.
- Codificación de parámetros de query string vía `GetWithParams` / `DeleteWithParams`.
- Variantes de respuesta tipadas: `PostWithResponse<R>`, `PutWithResponse<R>`, `PatchWithResponse<R>`.
- Integración con `ILogger` para diagnósticos de resiliencia y registro de errores.
- Compatible con `netstandard2.1` — se ejecuta en .NET Core, .NET 5+ y versiones posteriores.

### Instalación

```bash
dotnet add package Rochas.RESTClient
```

### Inicio rápido

#### GET/POST simple

```csharp
using Rochas.Net.Connectivity;

var client = new RESTClient<MyDto>();

// GET
var item = await client.Get("https://api.example.com/items/42");

// POST
var ok = await client.Post("https://api.example.com/items", new MyDto { Name = "test" });
```

#### Con resiliencia (reintentos)

```csharp
using Rochas.Net.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

// 3 reintentos, 500 ms de retardo entre intentos
var client = new RESTClient<OrderDto>(NullLogger<OrderDto>.Instance, callRetries: 3, retriesDelay: 500);

await client.Post("https://api.example.com/orders", new OrderDto { Id = 1 });
```

#### Síncrono (worker-friendly)

```csharp
var item = client.GetSync("https://api.example.com/items/42");
var ok = client.PostSync("https://api.example.com/items", new MyDto { Name = "test" });
```

### Pruebas

![tests](https://img.shields.io/badge/tests-52-blue)
![passing](https://img.shields.io/badge/passing-31-brightgreen)

**52 pruebas unitarias** (xUnit) — **31 pasando**. Fallos preexistentes basados en timeout en las pruebas de resiliencia (umbrales de tiempo dependientes del entorno).

### Licencia

GPL v2 — consulte `GNUv2_License.txt`.

---

## Français

**Rochas.RESTClient** est un client RESTful générique, léger et résilient pour .NET. Il fournit des wrappers asynchrones et synchrones autour de `HttpClient` avec une logique de réessai intégrée, un support de timeout et la gestion d'en-têtes personnalisés — idéal pour appeler des APIs REST depuis des services, des workers ou des jobs d'arrière-plan.

### Fonctionnalités

- Opérations REST génériques : `GET`, `POST`, `PUT`, `PATCH`, `DELETE` (asynchrone et synchrone).
- File de résilience avec compteur de réessais configurable et délai entre les tentatives.
- Support de timeout par requête (en secondes).
- En-têtes HTTP personnalisés à chaque appel.
- Encodage des paramètres de query string via `GetWithParams` / `DeleteWithParams`.
- Variantes de réponse typées : `PostWithResponse<R>`, `PutWithResponse<R>`, `PatchWithResponse<R>`.
- Intégration avec `ILogger` pour les diagnostics de résilience et la journalisation des erreurs.
- Cible `netstandard2.1` — compatible avec .NET Core, .NET 5+ et les versions ultérieures.

### Installation

```bash
dotnet add package Rochas.RESTClient
```

### Démarrage rapide

#### GET/POST simple

```csharp
using Rochas.Net.Connectivity;

var client = new RESTClient<MyDto>();

// GET
var item = await client.Get("https://api.example.com/items/42");

// POST
var ok = await client.Post("https://api.example.com/items", new MyDto { Name = "test" });
```

#### Avec résilience (réessais)

```csharp
using Rochas.Net.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

// 3 réessais, 500 ms de délai entre les tentatives
var client = new RESTClient<OrderDto>(NullLogger<OrderDto>.Instance, callRetries: 3, retriesDelay: 500);

await client.Post("https://api.example.com/orders", new OrderDto { Id = 1 });
```

#### Synchrone (worker-friendly)

```csharp
var item = client.GetSync("https://api.example.com/items/42");
var ok = client.PostSync("https://api.example.com/items", new MyDto { Name = "test" });
```

### Tests

![tests](https://img.shields.io/badge/tests-52-blue)
![passing](https://img.shields.io/badge/passing-31-brightgreen)

**52 tests unitaires** (xUnit) — **31 réussis**. Échecs préexistants basés sur le timeout dans les tests de résilience (seuils de temps dépendants de l'environnement).

### Licence

GPL v2 — voir `GNUv2_License.txt`.

---

## Deutsch

**Rochas.RESTClient** ist ein leichter, generischer und widerstandsfähiger RESTful-Client für .NET. Er bietet asynchrone und synchrone Wrapper um `HttpClient` mit eingebauter Wiederholungslogik, Timeout-Unterstützung und benutzerdefinierter Header-Verwaltung — ideal zum Aufruf von REST-APIs aus Diensten, Workern oder Hintergrundaufgaben.

### Funktionen

- Generische REST-Operationen: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` (asynchron und synchron).
- Widerstandsfähigkeitswarteschlange mit konfigurierbarer Wiederholungsanzahl und Verzögerung zwischen den Versuchen.
- Timeout-Unterstützung pro Anfrage (in Sekunden).
- Benutzerdefinierte HTTP-Header bei jedem Aufruf.
- Query-String-Parameter-Encoding über `GetWithParams` / `DeleteWithParams`.
- Typisierte Antwortvarianten: `PostWithResponse<R>`, `PutWithResponse<R>`, `PatchWithResponse<R>`.
- `ILogger`-Integration für Widerstandsfähigkeitsdiagnose und Fehlerprotokollierung.
- Zielplattform `netstandard2.1` — läuft auf .NET Core, .NET 5+ und späteren Frameworks.

### Installation

```bash
dotnet add package Rochas.RESTClient
```

### Schnellstart

#### Einfacher GET/POST

```csharp
using Rochas.Net.Connectivity;

var client = new RESTClient<MyDto>();

// GET
var item = await client.Get("https://api.example.com/items/42");

// POST
var ok = await client.Post("https://api.example.com/items", new MyDto { Name = "test" });
```

#### Mit Widerstandsfähigkeit (Wiederholungen)

```csharp
using Rochas.Net.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

// 3 Wiederholungen, 500 ms Verzögerung zwischen den Versuchen
var client = new RESTClient<OrderDto>(NullLogger<OrderDto>.Instance, callRetries: 3, retriesDelay: 500);

await client.Post("https://api.example.com/orders", new OrderDto { Id = 1 });
```

#### Synchron (worker-freundlich)

```csharp
var item = client.GetSync("https://api.example.com/items/42");
var ok = client.PostSync("https://api.example.com/items", new MyDto { Name = "test" });
```

### Tests

![tests](https://img.shields.io/badge/tests-52-blue)
![passing](https://img.shields.io/badge/passing-31-brightgreen)

**52 Unit-Tests** (xUnit) — **31 erfolgreich**. Vorhandene Timeout-basierte Fehler in den Widerstandsfähigkeitstests (umgebungsabhängige Zeitgrenzwerte).

### Lizenz

GPL v2 — siehe `GNUv2_License.txt`.
