Test level footage as of early December: https://youtu.be/gNj2SkE29tU

This project uses Unity version 2018.3.0f2

Formatting:
For all function definitions, use CamelCase
For all internal variables, use mixedCase
For all externally accessed variables through a Get, use CamelCase
For functions beginning with a predicate, used mixedCase  (e.g. "isGrounded()", "isVisible()")

Best practice:
Avoid using GetComponent() whenever possible, and prefer caching needed components on Start or Awake, or passing in necessary components
Avoid using Find
Avoid using Camera.main
Warnings are errors - unless they are specifically due to not yet implemented code, they should be fixed immediately

Project-specific:
Objects in scenes should always be placed in positions of multiples of 0.5. Objects should never be placed at smaller increments
Always use Time.deltaTime/Time.fixedDeltaTime for checking how much time has passed in Update/FixedUpdate
If something should rely on real time, and not in-game time, used Time.unscaledDeltaTime
Nothing should directly touch timeScale except for the TimeScaleManager
