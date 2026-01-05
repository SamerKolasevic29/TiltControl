# TiltControl Platform

**Status:** Core Engine / Base Platform

TiltControl is a research and experimental platform built on .NET MAUI that allows you to read device sensors (accelerometer) and transform continuous sensor input into manipulable outputs. The platform serves as the foundation for multiple experiments, games, and control applications.

---

## Philosophy

TiltControl is designed as a **modular research lab**:

- **Master branch** contains the core sensor handling, normalization, and shared utilities.
- Each **experiment or project** is developed in its own branch, allowing independent development without affecting the core engine.
- **Tags** mark checkpoints or completed milestones for reference.

---

## Branches Overview

| Branch | Purpose | Latest Tag / Status |
|--------|---------|---------------------|
| `master` | Core engine & shared logic | BaseCheckPoint2.0 |
| `experiment/ui-controls` | UI experiments for manipulating objects with sensors | v1.0-disc-manipulation-complete |
| `project/tilting-game` | Prototype tilting game with physics-based collisions | PhysicsPrototype1.0 |
| `project/universal-controller` | UDP-based universal controller with 4-axis input | Work in Progress |

> Each branch has its own `README.md` detailing how to run, features, and screenshots.

---

## Tilting Demo (gif)
<img src="docs/demonstration.gif" alt="TiltControl Demo" width="200"/>

## Getting Started

Clone the repository:

```bash
git clone https://github.com/SamerKolasevic29/TiltControl.git
