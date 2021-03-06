﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTrafficControl
{


	private Intersection referenceIntersection;
	private CarTrafficManager trafficManager;
	private NavigationManager navigationManager;
	private IntersectionTrafficMath trafficMath;

	// for now assume we stop at all intersections
	private List<AbstractCar> carsAtThisIntersection = new List<AbstractCar> ();

	private AbstractCar currentlyOperatingCar = null;
	private Vector3 currentPosition, targetPosition, pivot;
	private Quaternion currentRotation, targetRotation;
	private float startTime;

	public IntersectionTrafficControl (Intersection intersection, CarTrafficManager trafficManager, 
	                                  NavigationManager navigationManager, IntersectionTrafficMath trafficMath)
	{
		this.referenceIntersection = intersection;
		this.trafficManager = trafficManager;
		this.navigationManager = navigationManager;
		this.trafficMath = trafficMath;
	}

	public void tick ()
	{
		// check if there are cars waiting to go through the intersection
		if (currentlyOperatingCar == null) {

			if (carsAtThisIntersection.Count != 0) {
				currentlyOperatingCar = carsAtThisIntersection [0];
				carsAtThisIntersection.RemoveAt (0);
				// remove the car from the road // TODO: Redesign the communication between road control and intersection control
				trafficManager.roadControlForRoad(currentlyOperatingCar.position.referenceRoad).RemoveCarFromRoad(currentlyOperatingCar);
				
				
				// calculate the final form after going through the intersection
				Road targetRoad = navigationManager.targetWayForAbstractCarAtIntersection (currentlyOperatingCar, referenceIntersection);
				
				// get target abstract position
				AbstractCarPosition targetAbstractPosition = new AbstractCarPosition (targetRoad, referenceIntersection, 
					0, currentlyOperatingCar.position.laneNumber);
				float startingDistance = trafficMath.startingDistanceForCurrentDrive (targetAbstractPosition);
				targetAbstractPosition.offset = startingDistance;
				// get current and target real location
				currentPosition = currentlyOperatingCar.carGameObject.transform.position;
				currentRotation = currentlyOperatingCar.carGameObject.transform.rotation;
				currentlyOperatingCar.position = targetAbstractPosition;
				currentlyOperatingCar.CalculatePositionAndOrientation (out targetPosition, out targetRotation);
				// calculate the pivot
				Math3d.LineLineIntersection (out pivot, currentPosition,
					Quaternion.Euler (0, 90, 0) * currentRotation * Vector3.right,
					targetPosition, Quaternion.Euler (0, 90, 0) * targetRotation * Vector3.right);
				startTime = Time.time;
			}
		} else {
			// animate
			// very little animation if it is just a curve
			float turningDuration = referenceIntersection.GetGraphicsType() == Intersection.IntersectionGraphicsType.TWO_WAY_SMOOTH ? 
				0.1f : 1f;
			// end if necessary
			if (Time.time - startTime > turningDuration) {
				// we end turning and proceed to the following route
				RoadTrafficControl roadControl = trafficManager.roadControlForRoad (currentlyOperatingCar.position.referenceRoad);
				roadControl.AddCar (currentlyOperatingCar);
				// remember intersections is updated after roads
				currentlyOperatingCar.updateCarPosition ();
				currentlyOperatingCar = null;
				return;
			}
			// Slerp between positions around pivot and lerp the Quaternion rotation
			currentlyOperatingCar.carGameObject.transform.position
				= pivot + Vector3.Slerp (currentPosition - pivot, targetPosition - pivot, (Time.time - startTime) / turningDuration);
			currentlyOperatingCar.carGameObject.transform.rotation
				= Quaternion.Slerp (currentRotation, targetRotation, (Time.time - startTime) / turningDuration);

		}
		
	}

	

	// this method add cars to the processing queue only, it does not do anything with the car i.e. changing position etc
	public void AddCar (AbstractCar car)
	{
		this.carsAtThisIntersection.Add (car);
		
	}
}
