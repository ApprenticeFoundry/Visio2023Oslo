# Reinventing Visio in 2023:  A Blazor Project

### Business Needs to Draw

- Inspired by great 2d drawing software,  Todays discussion techniques in creating drawing software.  
- Two Boxes and one Arrow have meaning, its more than a picture
- We will discuss how I go about learning new technologies
- What attracted me to Visio (not drawing but design)
  - All the Shapes are Driven by a SpreadSheet (Parametric)
  - Drag and Drop from Stencils 
  - Shapes with units,  Pages with scaled images,  Drawing with different size pages
  - 1D Shapes can Glue to 2D Shapes , 2D shapes can Glue together Also
  - The Drawing Process can be automated using VB or C#
  - Visio Drawing Surface can be embedded into other applications
- Slide about Chris Roth  Visio Guy
- All the alternatives that were inspired by Visio 
- Love for D3 and network algorithms / rule the world

#### Four Box graph on new Domain vs new Technology  (known vs new)
- Known Domain & Known Technology: Earn a Living
- Known Domain & New Technology: Learning, prepare for the future
- New Domain & Known Technology: Consulting and Growing
- New Domain & New Technology:  Fake it till you make it

#### MUST keep Drawing and Semantic Model Separate
- Users cannot have their important data trapped inside of the shapes
- Model Elements / Drawing Elements are not 1:1
- Automated Tools / Frameworks for positioning and layout
- Alternative Views for the same Semantic Model 


### Learn a New Technology Every Christmas Season (Nearly)
- Silverlight / WPF
- JavaScript / HTML5 Canvas
- Angular / Typescript / HTML5 Canvas   (KnowtShare)
- Angular / Typescript / Three.js
- React / Typescript / HTML5 Canvas
- React / Typescript / Three.js
- C# / Unity
- C++ / UnReal
- Blazor  / HTML5 Canvas    

## Product Configuration:  What Visio does well and what need to be added

###  10 Behaviors that define Visio
- Parametric 1D/2D Shape Design puts the Developer in charge 
- Smart Drawing with Connected Shapes
- Shapes and drawings with units
- Complex Shape, including Groups, Connection Points and Handles
- Complete COM API for drawing automation and event capture
- Work can be completed Off-line
- Import Prior Art to extend / enrich / enhance
- Do / Undo / Redo   UI for visual interaction
- Formula Dependency Tracking  (smart formula evaluation)

###  10 Extensions we need in 2023 
- Modern Media Integration 
- Web / Cloud Deployment
- Animations
- True Collaboration using SignalR
- Persistence using Open Json model
- Separate Drawing Model from  the Semantic Model (this is hard)
- Seamless integration with 3D Shapes
- Drawing Version Control 


###  Innovations in this Iteration  
- Quad hit test algorithm  (Learned while doing object detection)
- Class for defining  Down / Move / Up operations as a set
- Rendering Time Slice Management  - Queue up mouse events
- Internal Event source and Syncs
- Mixing Inherence / Composition and Closures to simplify coding 
- Keeping a Command Stack,  Command is a First order object

## Early Version => Later Version
- Shape Data Structures
  - Early: Reference to Parent,  List of Childern 
  - Lessons: hard to serialize, Funct vs property, linerar lookup
  - Later: Dynamic <Generic> Slots, FoCollection grouping like types
- Shape Rendering 
  - Early: Render Method coded to geometry, forced Inheritance for new render
  - Lessons: Funct to draw, Methods to render, managing order
  - Later: shared draw library, Funct for rendering state (hover,selected)
  - Innovation: contextLink, prerender, postrender, Dynamic Definition
  - : RenderDetailed and RenderConcise
- Hit Testing / inverse transforms 
- Event listening
  - Early:  Listening to events
  - Later: internal PubSub Subscription network with pattern matching 
- Save and Restore
- Commanding / Messaging / RX /PubSub
- Value cashing / Recalculating (detailed/Concise/smashing)
- Scaled drawing pixel vs MKS
- Workspaces and Workpieces

## Sample Code and Live Demos
- the evolution of the render method  WPF / JavaScript / TypeScript / Blazor
- Workspaces and WorkPieces (Stencils)




