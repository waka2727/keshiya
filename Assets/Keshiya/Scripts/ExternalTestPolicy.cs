using System;
using UnityEngine;
namespace Keshiya {
 public static class ExternalTestPolicy {
  public const string Version="External Test 01";
  public static bool LegacyHarness(string[] args){foreach(string a in args)if(a.EndsWith("-test")&&a!="--external01-test")return true;return false;}
  public static bool DeveloperTools=>Application.isEditor||LegacyHarness(Environment.GetCommandLineArgs());
 }
}