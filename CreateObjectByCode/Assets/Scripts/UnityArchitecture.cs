using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityArchitecture : MonoBehaviour
{
    string nameArchitecture;
    int numberOfFloor;
    string typeOfRoof;
    List<UnityFloor> listFloor = new List<UnityFloor>();

    public string NameArchitecture { get => nameArchitecture; set => nameArchitecture = value; }
    public int NumberOfFloor { get => numberOfFloor; set => numberOfFloor = value; }
    public string TypeOfRoof { get => typeOfRoof; set => typeOfRoof = value; }
    public List<UnityFloor> ListFloor { get => listFloor; set => listFloor = value; }
}
