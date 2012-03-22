// 
// PListSchemeTest.cs
//  
// Author:
//       alanmcgovern <${AuthorEmail}>
// 
// Copyright (c) 2012 alanmcgovern
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

using MonoDevelop.MacDev;
using MonoDevelop.MacDev.PlistEditor;

namespace MonoDevelop.MacDev.Tests
{
	[TestFixture]
	public class PListSchemeTest
	{
		[Test]
		public void ArrayKey_WithArrayType ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" type = ""Array"" arrayType = ""Dictionary"" />
</PListScheme>");
			
			var key = scheme.GetKey ("keyname");
			Assert.AreEqual ("Array", key.Type, "#1");
			Assert.AreEqual ("Dictionary", key.ArrayType, "#2");
		}

		[Test]
		public void ArrayKey_WithoutArrayType ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" type = ""Array"" />
</PListScheme>");
			
			var key = scheme.GetKey ("keyname");
			Assert.AreEqual ("Array", key.Type, "#1");
			Assert.IsNull (key.ArrayType, "#2");
		}

		[Test]
		public void DictionaryKey_ValueDescriptions ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" type = ""Dictionary"" >
		<DictionaryValue _description = ""Foo""  />
		<DictionaryValue _description = ""Bar""  />
	</Key>
</PListScheme>
");
			var key = scheme.GetKey ("keyname");
			Assert.AreEqual ("Foo", key.Values [0].Description, "#1");
			Assert.AreEqual ("Bar", key.Values [1].Description, "#2");
		}

		[Test]
		public void DictionaryKey_ValueRequired ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" type = ""Dictionary"" >
		<DictionaryValue name = ""dict1"" valueType = ""String"" required=""True""  />
	</Key>
</PListScheme>
");
			var key = scheme.GetKey ("keyname");
			Assert.IsTrue (key.Values [0].Required, "#1");
		}

		[Test]
		public void DictionaryKey_WithValues ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" >
		<DictionaryValue name = ""dict1"" valueType = ""String""  />
		<DictionaryValue name = ""dict2"" valueType = ""Dictionary"" />
	</Key>
</PListScheme>
");
			Assert.AreEqual (1, scheme.Keys.Count, "#1");
			
			var key = scheme.GetKey ("keyname");
			Assert.AreEqual (2, key.Values.Count, "#2");
			Assert.IsInstanceOf<PListScheme.DictionaryValue> (key.Values [0], "#3");
			Assert.IsInstanceOf<PListScheme.DictionaryValue> (key.Values [1], "#4");
			
			var first = (PListScheme.DictionaryValue) key.Values [0];
			Assert.AreEqual ("dict1", first.Identifier, "#5");
			Assert.AreEqual ("String", first.ValueType, "#6");
			
			var second = (PListScheme.DictionaryValue) key.Values [1];
			Assert.AreEqual ("dict2", second.Identifier, "#7");
			Assert.AreEqual ("Dictionary", second.ValueType, "#8");
		}
		
		[Test]
		public void NumberKey_WithFullValues ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" _description = ""text"" type = ""Number"" >
		<Value name = ""1"" /> 
		<Value name = ""2"" /> 
	</Key>
</PListScheme>");

			var key = scheme.GetKey ("keyname");
			Assert.AreEqual (2, key.Values.Count, "#1");
			Assert.AreEqual ("1", key.Values [0].Identifier, "#2");
			Assert.AreEqual ("2", key.Values [1].Identifier, "#3");
		}

		[Test]
		public void StringKey ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" _description = ""text"" type = ""String""/>
</PListScheme>");
			Assert.AreEqual (1, scheme.Keys.Count, "#1");
			
			var key = scheme.GetKey ("keyname");
			Assert.IsNull (key.ArrayType, "#2");
			Assert.AreEqual ("text", key.Description, "#3");
			Assert.AreEqual ("keyname", key.Identifier, "#4");
			Assert.AreEqual ("String", key.Type, "#5");
			Assert.AreEqual (0, key.Values.Count, "#6");
		}

		[Test]
		public void StringKey_Descriptions ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname1"" _description = ""text1"" type = ""String""/>
	<Key name = ""keyname2"" _description = ""text2"" type = ""String""/>
</PListScheme>");
			Assert.AreEqual (2, scheme.Keys.Count, "#1");
			
			var key = scheme.GetKey ("keyname1");
			Assert.AreEqual ("text1", key.Description, "#2");
			
			key = scheme.GetKey ("keyname2");
			Assert.AreEqual ("text2", key.Description, "#3");
		}

		[Test]
		public void StringKey_WithFullValues ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" _description = ""text"" type = ""String"" >
		<Value name = ""ValidValue1"" _description = ""desc1"" /> 
		<Value name = ""ValidValue2"" _description = ""desc2"" required=""True"" /> 
	</Key>
</PListScheme>");

			var key = scheme.GetKey ("keyname");
			Assert.AreEqual (2, key.Values.Count, "#1");
			Assert.AreEqual ("ValidValue1", key.Values [0].Identifier, "#2");
			Assert.AreEqual ("desc1", key.Values [0].Description, "#3");
			Assert.AreEqual ("ValidValue2", key.Values [1].Identifier, "#4");
			Assert.AreEqual ("desc2", key.Values [1].Description, "#5");
			Assert.IsTrue (key.Values [1].Required, "#6");
		}

		[Test]
		public void StringKey_WithPartialValue ()
		{
			var scheme = Load (@"
<PListScheme>
	<Key name = ""keyname"" _description = ""text"" type = ""String"" >
		<Value name = ""ValidValue1"" /> 
	</Key>
</PListScheme>");

			var key = scheme.GetKey ("keyname");
			Assert.AreEqual (1, key.Values.Count, "#1");
			Assert.IsInstanceOf<PListScheme.Value> (key.Values [0], "#2");
			Assert.AreEqual ("ValidValue1", key.Values [0].Identifier, "#3");
			Assert.IsNull (key.Values [0].Description, "#4");
		}

		PListScheme Load (string value)
		{
			using (var reader = XmlReader.Create (new StringReader (value)))
				return PListScheme.Load (reader);
		}
	}
}

