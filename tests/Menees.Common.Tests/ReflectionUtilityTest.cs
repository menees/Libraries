using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ReflectionUtilityTest
	{
		[TestMethod]
		public void GetCopyrightTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string actual = ReflectionUtility.GetCopyright(asm);
			actual.ShouldEqual("Copyright For Unit Test");
		}

		[TestMethod]
		public void GetNameOfTest()
		{
			GetNameOfTester test = new GetNameOfTester();
			test.TestGetNameOf("Must pass a parameter for testing.");
		}

		[TestMethod]
		public void GetNameOfCallerTest()
		{
			string actual = ReflectionUtility.GetNameOfCaller();
			actual.ShouldEqual("GetNameOfCallerTest");

			GetNameOfTester.CallerName.ShouldEqual("CallerName");
		}

		private sealed class GetNameOfTester
		{
			private readonly string name;

			public GetNameOfTester()
			{
				this.name = "Tester";
			}

			public static string CallerName
			{
				get
				{
					string result = ReflectionUtility.GetNameOfCaller();
					return result;
				}
			}

			public string Name => this.name;

			public void SayHello()
			{
				this.SayHelloTo(this.name);
			}

			public void SayHelloTo(string target)
			{
				Log.Info(typeof(GetNameOfTester), "Hello, " + target);
			}

			public double GetSqrtPi() => Math.Sqrt(Math.PI);

			public void TestGetNameOf(string paramName)
			{
				// Get the name of a local variable.
				int localVariable = 0;
				string actual = ReflectionUtility.GetNameOf(() => localVariable);
				actual.ShouldEqual("localVariable");

				// Get the name of a parameter.
				actual = ReflectionUtility.GetNameOf(() => paramName);
				actual.ShouldEqual("paramName");

				// Get the name of a property.
				actual = ReflectionUtility.GetNameOf(() => Name);
				actual.ShouldEqual("Name");

				// Get the name of a field.
				actual = ReflectionUtility.GetNameOf(() => name);
				actual.ShouldEqual("name");

				// Get the name of a method.
				actual = ReflectionUtility.GetNameOf(() => SayHello());
				actual.ShouldEqual("SayHello");
				actual = ReflectionUtility.GetNameOf(() => SayHelloTo("Bill"));
				actual.ShouldEqual("SayHelloTo");
				actual = ReflectionUtility.GetNameOf(() => GetSqrtPi());
				actual.ShouldEqual("GetSqrtPi");

				// Get the name of a type.
				actual = ReflectionUtility.GetNameOf<GetNameOfTester>();
				actual.ShouldEqual("GetNameOfTester");
			}
		}

		[TestMethod]
		public void GetBuildTimeTest()
		{
			// The Menees.Common.Tests assembly is built with Deterministic = false, so it will have a build time.
			Assembly assembly = Assembly.GetExecutingAssembly();
			DateTime? built = ReflectionUtility.GetBuildTime(assembly);
			built.ShouldNotBeNull("Menees.Common.Tests assembly");
			FileInfo fileInfo = new FileInfo(assembly.Location);
			fileInfo.Exists.ShouldBeTrue();
			DateTime lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
			TimeSpan timeDiff = lastWriteTimeUtc - built.Value;
			Math.Abs(timeDiff.TotalSeconds).ShouldBeLessThan(60);

			// The Menees.Common assembly is built with Deterministic = true, so it will NOT have a build time.
			built = ReflectionUtility.GetBuildTime(typeof(Conditions).Assembly);
			built.ShouldBeNull("Menees.Common assembly");
		}
	}
}
