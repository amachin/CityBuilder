﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I realized that Road Traffic Control is designed to fulfill the purpose of car traffic control
// This class is ISOLATED until further usage are discovered
public class CarTrafficManager : MonoBehaviour
{

	public float carSpeed;

	private List<AbstractCar> cars = new List<AbstractCar> ();

	public void AddCar (AbstractCar car)
	{
		Debug.Assert (!cars.Contains (car));
        
		cars.Add (car);
		roadControlForRoad(car.position.referenceRoad).AddCar(car);
	}

	public void Update ()
	{
		foreach (IntersectionTrafficControl control in allIntersectionControls)
		{
			control.tick();
		}

		foreach (RoadTrafficControl control in allRoadControls)
		{
			control.tick();
		}
		
	}
	
	
	private List<Intersection> allIntersections = new List<Intersection>();
	private List<Road> allRoads = new List<Road>();
	private List<IntersectionTrafficControl> allIntersectionControls = new List<IntersectionTrafficControl>();
	private List<RoadTrafficControl> allRoadControls = new List<RoadTrafficControl>();

	public void addNewIntersection(Intersection intersection)
	{
		allIntersections.Add(intersection);
		allIntersectionControls.Add(new IntersectionTrafficControl(intersection));
	}

	public IntersectionTrafficControl intersectionControlForIntersection(Intersection intersection)
	{
		return allIntersectionControls[allIntersections.IndexOf(intersection)];
	}

	public void addNewRoad(Road road)
	{
		allRoads.Add(road);
		allRoadControls.Add(new RoadTrafficControl(road));
	}

	public RoadTrafficControl roadControlForRoad(Road road)
	{
		return allRoadControls[allRoads.IndexOf(road)];
	}

}
