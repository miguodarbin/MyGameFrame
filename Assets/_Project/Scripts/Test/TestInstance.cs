using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestInstance: SingletonData<TestInstance>
{
   public void Test()
   {
      Debug.Log("Test");
   }
}