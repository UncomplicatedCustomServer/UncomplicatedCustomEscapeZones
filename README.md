<div align="center"><a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/releases/latest"><img src="https://img.shields.io/github/v/release/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/releases/latest"><img src="https://img.shields.io/github/downloads/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/total"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/pulls"><img src="https://img.shields.io/github/issues-pr/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/pulls"><img src="https://img.shields.io/github/issues-pr-closed/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones"></a> <a href="https://opencollective.com/ucs"><img src="https://img.shields.io/opencollective/all/ucs?label=OpenCollective%20backers&color=7FADF2"></a>

  <h1>UncomplicatedCustomEscapeZones</h1>
  <i>Easy, fully configurable and customizable escape zones for your SCP:SL Server!</i>

  <br><br>
  <b><a href="https://docs.ucez.ucserver.it">📑 Read the documentation</a></b>

</div>

## Requirements

- **LabAPI** >= `v1.1.7`
- **UncomplicatedCustomRoles** (optional) — only needed if you want to use UCR Custom Roles inside your escape zones
- **UncomplicatedCustomTeams** (optional) — only needed if you want to match your escape zones against UCT Custom Teams

<br>

## What's UncomplicatedCustomEscapeZones

**UncomplicatedCustomEscapeZones** or **UCEZ** is a plugin for **LabAPI** that lets you replace the vanilla escape zone
with as many custom ones as you want, defined with YAML.\
An escape zone is just a box somewhere on the map: you decide where it is, how big it is, who is allowed to escape
through it and which role every escaping player becomes — including UCR Custom Roles.\
The default SCP:SL escape zone is removed when the plugin loads, so your zones are the only way out.

## Features

### 🗺️ Escape zones wherever you want

A zone is a box (a center and a size) that you can either pin to a room — so it follows that room wherever the map
generator puts it — or place at absolute map coordinates. Create as many of them as you like, each one in its own `yml`
file.

### 🎭 Full control over what a player becomes

For every team, faction, role or UCR Custom Role you can decide what happens on escape: turn into another vanilla role,
turn into a UCR Custom Role, or simply be denied the escape.

### 🔗 Different outcomes when cuffed

The result can change depending on **who** disarmed the escaping player — by team, by faction, by vanilla role or by UCR
Custom Role — so a cuffed Class-D can become an NTF Private while a free one becomes a Chaos Conscript.

### 🧩 UncomplicatedCustomRoles and UncomplicatedCustomTeams integration

UCEZ talks to **UCR** out of the box: use your UCR Custom Role Ids both as the condition (who is escaping / who cuffed
them) and as the result of the escape. The **CustomTeam** module is supported as well, so an entire custom team can
share one escape configuration, and a role that fakes its team is evaluated with the team it is faking.\
**UCT** Custom Teams work the same way: match a whole team by its name or by its Id. Both plugins are soft
dependencies — they are only needed if you actually use them in a config.

### 🌐 Per-port configurations

Every zone folder exists both globally and per server port, so servers sharing the same config folder can have their own
escape zones.

### ⌨️ In-game commands

Manage your escape zones from the Remote Admin console: `ucez reload`, `ucez outline` and `ucez getposition`.

### 🗂️ YAML based

You don't need to know how to code to use UCEZ: escape zones are created using `yml`
([YAML](https://en.wikipedia.org/wiki/YAML)) files, an extremely easy and intuitive serialization language!

### 📑 Exhaustive documentation

Check out the [official UCEZ documentation](https://docs.ucez.ucserver.it/), where you'll find everything you need to
use the plugin: from getting started to advanced settings!

## Installation

1. Download the latest release from
   the [GitHub Releases](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/releases/latest).
2. Place the `.dll` file into your `LabApi/plugins/global` directory.
3. Restart your server to generate the configuration files.
4. Configure your custom escape zones in the generated configuration files.

The [documentation](https://docs.ucez.ucserver.it/) walks you through the same steps in detail.

## If you use UCEZ, please consider making a donation

UCEZ is a plugin made by **UCS Collective**.\
Every plugin we create is **free** and **open-source**, and it always will be.\
What there is, is the time we spend writing the plugin, answering your questions on Discord and keeping everything
working after every SCP:SL update.\
If UCEZ is running on your server, **please consider donating something through OpenCollective** — every contribution,
however small, goes straight back into the plugins you are using:

<a href="https://opencollective.com/ucs"><img height="15" src="https://raw.githubusercontent.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/refs/heads/resources/oc_icon.png">&nbsp;&nbsp;Donate</a>

## Contacts

### UCS - UncomplicatedCustomServer

**Documentation:** [https://docs.ucez.ucserver.it](https://docs.ucez.ucserver.it)\
**Discord:** [https://discord.gg/5StRGu8EJV](https://discord.gg/5StRGu8EJV)

### FoxWorn3365

**Discord:** `@foxworn`\
**Email:** `foxworn3365@gmail.com`

### MedveMarci

**Discord:** `@medvemarci`
