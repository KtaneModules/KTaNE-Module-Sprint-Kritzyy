using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModuleSprintExtras
{
    public class AnimationHandler
    {
        public bool Ongoing { get; private set; }

        public AnimationHandler()
        {
            Ongoing = false;
        }

        public IEnumerator HandleShutterAnimation(string Command, GameObject TopShutter, GameObject BottomShutter) // Commands: "open" only opens, "close" only closes, "cycle" does both
        {
            // Animation Start
            Ongoing = true;

            // Closing animation
            if (Command != "Open")
            {
                for (int T = 0; T < 20; T++)
                {
                    // Top shutter. Start position = 0.0189, end = 0.0574. 20 steps of 0.001925
                    TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, (float)(0.0574f - (0.001925f * T)));

                    // Bottom shutter. Start position = -0.0227, end = -0.0601. 20 steps of -0.00187
                    BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, (float)(-0.0601f - (-0.00187f * T)));

                    yield return new WaitForSecondsRealtime(0.0125f);
                }
                // Just for good measure
                TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, 0.02017f);
                BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, -0.02397f);
            }
            
            if (Command == "Cycle")
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }

            // Opening animation
            if (Command != "Close")
            {
                for (int T = 0; T < 20; T++)
                {
                    // Top shutter. Start position = 0.0189, end = 0.0574. 20 steps of 0.001925
                    TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, (float)(0.0189f + (0.001925f * T)));

                    // Top shutter. Start position = -0.0227, end = -0.0601. 20 steps of -0.00187
                    BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, (float)(-0.0227f + (-0.00187f * T)));

                    yield return new WaitForSecondsRealtime(0.0125f);
                }
                // Just for good measure
                TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, 0.0574f);
                BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.0115f, -0.0601f);
            }

            // Animation end
            Ongoing = false;
        }

        public IEnumerator HandlePlatformAnimation(string Command, GameObject InteriorPlatform, GameObject ModuleToDisable, GameObject ModuleToEnable) // Commands: "open" only opens, "close" only closes, "cycle" does both
        {
            // Animation Start
            Ongoing = true;

            // Closing animation
            if (Command != "Open")
            {
                for (int T = 0; T < 20; T++)
                {
                    // Start position = 0.0059, end = -0.0125. 20 steps of -0.00092
                    InteriorPlatform.transform.localPosition = new Vector3(-0.0214f, (float)(0.0059 + (-0.00092f * T)), -0.0013f);

                    yield return new WaitForSecondsRealtime(0.0125f);
                }
                // Just for good measure
                InteriorPlatform.transform.localPosition = new Vector3(-0.0214f, -0.0125f, -0.0013f);
                InteriorPlatform.SetActive(false);
                ModuleToDisable.SetActive(false);
            }

            if (Command == "Cycle")
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }

            // Opening animation
            if (Command != "Close")
            {
                InteriorPlatform.SetActive(true);
                ModuleToEnable.SetActive(true);
                for (int T = 0; T < 20; T++)
                {
                    // Start position = -0.0125, end = 0.0059. 20 steps of 0.00092
                    InteriorPlatform.transform.localPosition = new Vector3(-0.0214f, (float)(-0.0125f + (0.00092f * T)), -0.0013f);

                    yield return new WaitForSecondsRealtime(0.0125f);
                }
                // Just for good measure
                InteriorPlatform.transform.localPosition = new Vector3(-0.0214f, 0.0059f, -0.0013f);
            }

            // Animation End
            Ongoing = false;
        }

        public IEnumerator HandleForceSolveAnimation(GameObject TopShutter, GameObject BottomShutter)
        {
            for (int T = 0; T < 20; T++)
            {
                // Top shutter. Start position = 0.0189, end = 0.0574. 20 steps of 0.001925
                TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.012f, (float)(0.0574f - (0.001925f * T)));

                // Bottom shutter. Start position = -0.0227, end = -0.0601. 20 steps of -0.00187
                BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.012f, (float)(-0.0601f - (-0.00187f * T)));

                yield return new WaitForSecondsRealtime(0.0125f);
            }
            // Just for good measure
            TopShutter.transform.localPosition = new Vector3(-0.0208f, 0.012f, 0.0194f);
            BottomShutter.transform.localPosition = new Vector3(-0.0208f, 0.012f, -0.024f);
        }
    }
}
