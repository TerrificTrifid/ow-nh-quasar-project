using NewHorizons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.Utility;
using UnityEngine;

namespace QuasarProject
{
    [UsedInUnityProject]
    public class QuasarRenderController : MonoBehaviour
    {
        

        public void Render()
        {
            // if this is closer to than "accretiondisktest", render it
            if (Physics.Raycast(Locator.GetActiveCamera().transform.position,Vector3.MoveTowards(Locator.GetActiveCamera().transform.position, transform.position, 1f), out var raycastHit, 10000f))
            {
                if (raycastHit.collider.gameObject.name == "accretiondisktest")
                {
                    GetComponent<MeshRenderer>().material.renderQueue = 2999;
                }
                else
                {
                    GetComponent<MeshRenderer>().material.renderQueue = 3001;
                }
            }
            else
            {
                GetComponent<MeshRenderer>().material.renderQueue = 3001;
            }

            
            
           


        }


    }
}
