/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System;
using System.Collections;
using SpiritAI.CharacterEngine.Network;
using SpiritAI.CharacterEngine.WorkQueue;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public class NetworkState : INetworkState
    {
        public class TestConnectionWorkItem : WorkItem
        {
            public NetworkState NetworkState { get; set; }

            protected override IEnumerator DoWork(WorkManager.Work item)
            {
                var status = UnityEngine.Network.TestConnection();
                yield return null;
                switch (status)
                {
                    case ConnectionTesterStatus.Error:
                        NetworkState.NetworkStatus = NetworkStatus.Error;
                        break;
                    case ConnectionTesterStatus.Undetermined:
                        NetworkState.NetworkStatus = NetworkStatus.Unknown;
                        break;
                    /** Deprecated in Unity
                                            case ConnectionTesterStatus.PrivateIPNoNATPunchthrough:
                                                NetworkState.NetworkStatus = NetworkStatus.Connected;
                                                break;
                                            case ConnectionTesterStatus.PrivateIPHasNATPunchThrough:
                                                NetworkState.NetworkStatus = NetworkStatus.Connected;
                                                break;
*/
                    case ConnectionTesterStatus.PublicIPIsConnectable:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.PublicIPPortBlocked:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.PublicIPNoServerStarted:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.NATpunchthroughFullCone:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
                        NetworkState.NetworkStatus = NetworkStatus.Connected;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public WorkItem TestConnection()
        {
            NetworkStatus = NetworkStatus.Untested;
            var item = new TestConnectionWorkItem()
            {
                NetworkState = this,
            };
            WorkManager.Enqueue(item, ThreadType.Foreground);
            return item;
        }

        public NetworkStatus NetworkStatus { get; private set; }
    }
}
