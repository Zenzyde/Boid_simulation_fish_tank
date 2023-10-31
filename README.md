# Boid_simulation_fish_tank

This is a simpler simulation of the AI concept of "boids" or units moving around based on multiple, simple steering behaviours.
I have chosen to visualize this as a "fish tank", using some nice cyan walls and a blue floor :)

The steering behaviours implemented in the simulation can be described as follows:
* Alignment
* Separation
* Cohesion
* Wander
* Avoid "fish tank"

The simulation itself is autonomous, but boids can be added and removed dynamically

The following screenshot shows the "fishes" in action
![Boids](/images/active_simulation_example.png)

The following screenshot shows an example of a scriptable object which controls the setting for the individual steering behaviours.
![Gridmanager](/images/boids_scriptable_object_align.png)

The following screenshot shows the main script controlling the simulation
![Boidmanager](/images/boids_manager_script.png)
