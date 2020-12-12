// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Zenject;

// public class ThemedObjectContainerFactory : IFactory<UnityEngine.Object, ThemedObjectContainer>
// {
//     readonly DiContainer _container;

//     public ThemedObjectFactory(DiContainer container)
//     {
//         _container = container;
//     }

//     public ThemedObjectContainer Create(UnityEngine.Object prefab)
//     {
//         ThemedObjectContainer newContainer = _container.InstantiatePrefabForComponent<ThemedObjectContainer>(prefab);
//         newContainer = 
//         return newContainer;
//     }
// }