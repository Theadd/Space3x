# Extended Attributes (Runtime UI & Non-Serialized)

## Overview

Inspector Attributes also for Non-Serialized properties and Runtime UI (UI Toolkit).

## Main Features

- **Serialized Properties**  
  It just uses Unity's built-in implementation, ensuring backwards compatibility with any existing code using it.
- **Non-Serialized Properties**  
  Can be displayed just like the serialized ones, including custom drawers and decorators, but for non-serialized properties, objects and any children properties, at any depth.
- **Runtime UI**  
  Automatically generate a full UI of VisualElements on-the-fly from an object and all its nested properties in depth, anywhere, even from UI Builder. In Runtime UI, all properties are rendered as Non-Serialized properties.

