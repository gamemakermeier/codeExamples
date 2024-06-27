using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ampere.Utility
{
    public class PhysicsUtility
    {
        public static void changeLinearDragForTime(MonoBehaviour responsibleMonoBehavior, Rigidbody2D targetRig, float targetDrag, float reductionTime, bool degradationOverTime)
        {
            //Debug.Log("startin change linear drag coroutine");
            responsibleMonoBehavior.StartCoroutine(changeLinearDragForTimeCR(targetRig, targetDrag, reductionTime, degradationOverTime));
        }
        private static IEnumerator changeLinearDragForTimeCR(Rigidbody2D targetRig, float targetDrag, float reductionTime, bool degradationOverTime)
        {
            float timer = 0;
            float oldDrag = targetRig.drag;
            targetRig.drag = targetDrag;
            if (degradationOverTime)
            {
                while (timer < reductionTime)
                {
                    targetRig.drag = Mathf.Lerp(targetDrag, oldDrag, timer / reductionTime);
                    timer += Time.deltaTime;
                    //Debug.Log($"Timer is {timer}");
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                yield return new WaitForSeconds(reductionTime);
            }
            targetRig.drag = oldDrag;
        }
    }
}
