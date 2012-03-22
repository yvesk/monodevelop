// 
// PListScheme.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2011 Xamarin <http://xamarin.com>
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
using System.Xml;
using System;
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.Core;

namespace MonoDevelop.MacDev.PlistEditor
{
	public partial class PListScheme
	{
		public class Value {
			public string Description { get; set; }
			public string Identifier { get; set; }
			public bool Required { get; set; }
			public List<Value> Values { get; private set; }
			
			public Value ()
			{
				Values = new List<Value> ();
			}
		}
		
		public class DictionaryValue : Value
		{
			public string ArrayType { get; set; }
			public string ValueType { get; set; }
		}
		
		public class Key {
			public string ArrayType { get; set; }
			public string Description { get; set; }
			public string Identifier { get; set; }
			public string Type { get; set; }
			public List<Value> Values { get; private set; }
			
			public Key ()
			{
				Values = new List<Value> ();
			}
		}
	}
	
	public partial class PListScheme
	{
		public static readonly PListScheme Empty = new PListScheme () { keys = new Key [0] };
		
		IList<Key> keys = new List<Key> ();

		public IList<Key> Keys {
			get {
				return keys;
			}
		}

		public Key GetKey (string id)
		{
			return keys.FirstOrDefault (k => k.Identifier == id);
		}

		public static PListScheme Load (XmlReader reader)
		{
			var result = new PListScheme ();
			var doc = new XmlDocument ();
			doc.Load (reader);
			
			foreach (XmlNode keyNode in doc.SelectNodes ("/PListScheme/*")) {
				var key = new Key {
					Identifier = AttributeToString (keyNode.Attributes ["name"]),
					Description = AttributeToString (keyNode.Attributes ["_description"]),
					Type = AttributeToString (keyNode.Attributes ["type"]),
					ArrayType = AttributeToString (keyNode.Attributes ["arrayType"])
				};
				
				if (keyNode.HasChildNodes)
					key.Values.AddRange (ParseValues (keyNode.ChildNodes));
				result.Keys.Add (key);
			}
			
			return result;
		}
		
		static IEnumerable<Value> ParseValues (XmlNodeList nodeList)
		{
			List<Value> values = new List<Value> ();
			foreach (XmlNode node in nodeList) {
				Value value = null;
				if (node.Name == "DictionaryValue") {
					value = new DictionaryValue {
						ArrayType = AttributeToString (node.Attributes ["arrayType"]),
						ValueType = AttributeToString (node.Attributes ["valueType"])
					};
				} else if (node.Name == "Value") {
					value = new Value ();
				} else {
					throw new NotSupportedException (string.Format ("Node of type {0} not supported as a Value", node.Name));
				}
				value.Description = AttributeToString (node.Attributes ["_description"]);
				value.Identifier = AttributeToString (node.Attributes ["name"]);
				if (node.Attributes ["required"] != null)
					value.Required = bool.Parse (node.Attributes ["required"].Value);
				
				if (node.HasChildNodes)
					value.Values.AddRange (ParseValues (node.ChildNodes));
				
				values.Add (value);
			}
			return values;
		}
		
		static string AttributeToString (XmlAttribute attr)
		{
			if (attr == null || string.IsNullOrEmpty (attr.Value))
				return null;
			return attr.Value;
		}
	}
}

