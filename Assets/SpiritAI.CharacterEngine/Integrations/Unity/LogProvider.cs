/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using SpiritAI.CharacterEngine.Logging;
using UnityEngine;
using LogType = SpiritAI.CharacterEngine.Logging.LogType;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public class LogProvider : MonoBehaviour, ILogProvider
    {
        public void DoFileUpload(byte[] data, string path, string elementName, string filename, string contentType, string uploadUrl)
        {
            StartCoroutine(UploadFile(data, path, elementName, filename, contentType, uploadUrl));
        }

        private IEnumerator UploadFile(byte[] data, string path, string elementName, string filename, string contentType,
            string uploadUrl)
        {
            var postForm = new WWWForm();
            postForm.AddBinaryData(elementName, data, filename, contentType);

            var upload = new WWW(uploadUrl, postForm);
            yield return upload;
            if (upload.error == null)
            {
                CEDebug.Log(LogLevel.Verbose, LogType.Information, upload.text);
                yield break;
            }

            CEDebug.Log(LogLevel.Normal, LogType.Error, "Upload Error: {0}", upload.error);
        }
    }
}
