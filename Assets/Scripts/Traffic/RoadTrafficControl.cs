﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadTrafficControl
{
	private Road referenceRoad;
    private IntersectionTrafficMath trafficMath;
    private CarTrafficManager trafficManager;
	private List<AbstractCar> cars = new List<AbstractCar>();

	public RoadTrafficControl (Road road, 
        IntersectionTrafficMath trafficMath, CarTrafficManager trafficManager)
	{
		this.referenceRoad = road;
		this.trafficMath = trafficMath;
		this.trafficManager = trafficManager;
	}

	public void AddCar(AbstractCar car)
	{
		cars.Add(car);
	}

	// this method should be called once per frame to update car's positions
	public void tick()
	{
		if (cars.Count == 0)
		{
			return;
			
		}
		
		// processing each lane seperately each direction seperately
		
		int maxLaneNumber = cars.Max(c => c.position.laneNumber);

		for (int i = 0; i < maxLaneNumber + 1; i++)
		{
			processCarsWithLaneNumberAndRefernceIntersection(i, referenceRoad.fromIntersection);
			processCarsWithLaneNumberAndRefernceIntersection(i, referenceRoad.toIntersection);
		}
		
		
		

	}

	private void processCarsWithLaneNumberAndRefernceIntersection(int laneNumber, Intersection refIntersection)
	{
		//TODO: sort list first instead of doing it on the fly
		AbstractCar[] sortedCars = cars
			.Where(c => c.position.referenceIntersection == refIntersection)
			.Where(c => c.position.laneNumber == laneNumber)
			.OrderByDescending(c => c.position.offset)
			.ToArray();
		for (int j = 0; j < sortedCars.Length; j++)
		{
			AbstractCar car = sortedCars[j];
			// check if this is the first car
			// TODO: Reduce code copying
			if (j == 0)
			{
				// move the first car into the intersection if necessary
				float stoppingOffset = trafficMath.stoppingDistanceForCurrentDrive(car.position);
				if (car.position.offset < stoppingOffset)
				{
					car.SmartAdjustSpeedAccordingToStoppingDistance(stoppingOffset - car.position.offset, 2f );
					car.position.offset += car.GetSpeed() * Time.deltaTime;
					//we should not exceed the stopping distance
					// If the car is close enough to the intersection
					if (stoppingOffset - car.position.offset < 0.05f)
					{
						car.position.offset = stoppingOffset;
						car.DecreaseSpeed(100f);// force stop the car

						moveCarToIntersection(car);
					}
				}

				car.updateCarPosition();
			}
			else
			{
				// let the car follow the previous car
				int MIN_DIST = 2;
				float stoppingOffset = sortedCars[j - 1].position.offset - MIN_DIST;
				if (car.position.offset < stoppingOffset)
				{
					car.SmartAdjustSpeedAccordingToStoppingDistance(stoppingOffset - car.position.offset, 2f );
					car.position.offset += car.GetSpeed() * Time.deltaTime;
				}

				car.updateCarPosition();
			}
		}
	}

	private void moveCarToIntersection(AbstractCar car)
	{
		Intersection toIntersection = car.position.referenceRoad.otherIntersection(car.position.referenceIntersection);
		IntersectionTrafficControl intersectionControl = trafficManager.intersectionControlForIntersection(toIntersection);
		intersectionControl.AddCar(car);
		// the car is still on our road until the intersection takes that car out

	}

	public void RemoveCarFromRoad(AbstractCar car)
	{
		Debug.Assert(cars.Contains(car));

		cars.Remove(car);
	}
	
}
