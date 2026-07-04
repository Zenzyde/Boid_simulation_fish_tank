# Boid fishes simulation

This is a simpler simulation of the AI concept of "boids" or units moving around based on multiple, simple steering behaviours.
I have chosen to visualize this as a "fish tank", using some nice cyan walls and a blue floor :)

This is version 2.0, an implementation using a hybrid approach of Unity ECS for the main simulation, and normal Unity Monobehavior for input handling
(Also the models for the fish are not made by me, nor are they AI generated)

The steering behaviours implemented in the simulation can be described as follows:
* Alignment
* Separation
* Cohesion
* Wander
* Avoid obstacle (which in this case is the "tank")

The simulation itself is autonomous, boids can be added and removed dynamically, and can be individually selected to view some info about the selected boid.

The following screenshots show the two different modes
![Boid spawn/delete mode](/images/Boid_simulation spawn_delete_mode.png)
![Boid selection mode](/images/Boid_simulation_selection_mode.png)

The following screenshot shows an example of a scriptable object which controls the setting for an individual "species" of boid.
![Boid species SO](/images/Boid_species_settings.png)

The following screenshot is an example of a steering behavior settings SO
![Boid steering behavior SO](/images/Alignment_behavior_settings.png)