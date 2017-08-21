﻿/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace ApiAiFormsSDK
{
	public static class Log
	{
		public static void Debug(string tag, string msg)
		{
            System.Diagnostics.Debug.WriteLine("{0}: {1}", tag, msg);
		}

		public static void Debug(string tag, string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine("{0}: {1}", tag, string.Format(format, args));
		}

		public static void Warn(string tag, string msg)
		{
			System.Diagnostics.Debug.WriteLine("{0}: {1}", tag, msg);
		}

		public static void Warn(string tag, string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine("{0}: {1}", tag, string.Format(format, args));
		}

		public static void Error(string tag, string msg)
		{
			System.Diagnostics.Debug.WriteLine("{0}: {1}", tag, msg);
		}

		public static void Error(string tag, string msg, Exception e)
		{
			System.Diagnostics.Debug.WriteLine("{0}: {1} {2}", tag, msg, e);
		}
	}
}
