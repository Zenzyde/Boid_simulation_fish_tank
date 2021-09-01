# Boid_simulation_fish_tank

This is a simpler simulation of the AI concept of "boids" or units moving around based on multiple, simple steering behaviours.
I have chosen to visualize this as a "fish tank", where the "tank" itself just so happens to be invisible :P

The steering behaviours implemented in the simulation can be described as follows:
* Alignment
* Separation
* Cohesion
* Wander
* Avoid "fish tank"

The simulation itself is autonomous, but boids can be added and removed dynamically

The following screenshot shows the "fishes" in action
![Boids](/images/active_simulation_145.png)

The following screenshot shows the custom made spatial query system in action
![Gridmanager](/images/active_simulation_5_inhabited_grid.png)

The following screenshot shows the main script controlling the simulation
![Boidmanager](/images/boids_manager_script.png)
