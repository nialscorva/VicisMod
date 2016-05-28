using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class DroneComputer {

        public const string LOGGER_PREFIX = "Vici.DroneComputer";

        protected GameObject drone;
        protected GameObject thrust;
        protected GameObject clamp;
        protected GameObject carriedItem;
        protected Vector3 dronePos;
        protected float speed;

        public DroneComputer(float speed) {
            this.speed = speed;
        }

        public void setDrone(GameObject drone) {
            VicisMod.log(LOGGER_PREFIX, "Drone pos is currently = " + dronePos + ", drone is = " + drone.transform.position);
            if (dronePos == Vector3.zero) {
                dronePos = drone.transform.position;
            } else {
                drone.transform.position = dronePos;
            }
            VicisMod.log(LOGGER_PREFIX, "Drone pos is now = " + dronePos);

            this.drone = drone;
        }

        public void setThrust(GameObject thrust) {
            this.thrust = thrust;
        }

        public void setClamp(GameObject clamp) {
            this.clamp = clamp;
        }

        public void flyToUnity(Vector3 point, float timeJump) {
            if (dronePos == Vector3.zero) return;
            Vector3 thrust = point - dronePos;
            thrust.Normalize();

            Vector3 newPos = dronePos + thrust * timeJump * speed;
            // VicisMod.log(LOGGER_PREFIX, "Aiming for " + point + ", thrust was " + thrust + ", modified thrust " + (thrust * timeJump * speed) + ", New position is " + newPos + ", currently at " + dronePos + ", timeJump = " + timeJump + ", speed = " + speed);

            // Detect if we passed the point
            float t1 = (newPos - dronePos).sqrMagnitude;
            float t2 = (point - dronePos).sqrMagnitude;
            if (t1 > t2) newPos = point;

            dronePos = newPos;

            if (drone != null) {
                drone.transform.position = newPos;
                // I *think* this is the rotation part of the drone script
                Vector3 rotation = thrust - drone.transform.forward;
                rotation.y = 0f;
                if (rotation != Vector3.zero) {
                    drone.transform.forward += rotation * Time.deltaTime * 2.5f;
                }
                if (this.thrust != null) {
                    this.thrust.SetActive(true);
                }
            }

        }

        public void flyToWorld(long x, long y, long z, float timeJump) {
            flyToUnity(getUnityCoords(x, y, z), timeJump);
        }

        public void goToUnity(Vector3 point) {
            if (dronePos == Vector3.zero) return;
            dronePos = point;
            if (drone != null) {
                drone.transform.position = point;
                Vector3 temp = point - dronePos;
                temp.Normalize();
                drone.transform.forward = temp;
                if (!thrust.activeSelf) {
                    thrust.SetActive(true);
                }
            }
        }

        public void goToWorld(long x, long y, long z) {
            goToUnity(getUnityCoords(x, y, z));
        }

        public void faceTo(Vector3 forwards) {
            if (drone == null) return;
            forwards.x += 0.1f;
            forwards.Normalize();
            drone.transform.forward += (forwards - drone.transform.forward) * Time.deltaTime * 0.5f;
            if (thrust.activeSelf) {
                thrust.SetActive(false);
            }
        }

        public void giveItem(ItemBase item) {
            if (this.clamp == null) return;
            if (carriedItem != null) {
                UnityEngine.Object.Destroy(carriedItem);
            }
            if (item == null || item.mnItemID == -1) return;
            int @object = (int)ItemEntry.mEntries[item.mnItemID].Object;
            GameObject original = SpawnableObjectManagerScript.instance.maSpawnableObjects[@object];
            carriedItem = (GameObject)UnityEngine.Object.Instantiate(original, clamp.transform.position, clamp.transform.rotation);
            carriedItem.transform.parent = clamp.transform;
            carriedItem.transform.localPosition = Vector3.zero;
            carriedItem.transform.rotation = Quaternion.identity;
            carriedItem.SetActive(true);
        }

        public Vector3 getUnityCoords(long x, long y, long z) {
            return WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(x, y, z);
        }

        public Vector3 getPos() {
            return dronePos;
        }

        public void delete() {
            drone = null;
            thrust = null;
            clamp = null;
        }
    }

}