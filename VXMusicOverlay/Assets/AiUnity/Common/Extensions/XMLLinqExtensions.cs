// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System.Collections;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// XML Linq Extensions.
    /// </summary>
    public static class XMLLinqExtensions
	{
        /// <summary>
        /// Moves the element up.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <exception cref="System.ArgumentException">Nowhere to move element to!</exception>
        public static void MoveElementUp (this XElement element)
		{
			// Walk backwards until we find an element - ignore text nodes
			XNode previousNode = element.PreviousNode;
			while (previousNode != null && !(previousNode is XElement)) {
				previousNode = previousNode.PreviousNode;
			}
			if (previousNode == null) {
				throw new ArgumentException ("Nowhere to move element to!");
			}
			element.Remove ();
			previousNode.AddBeforeSelf (element);
		}

        /// <summary>
        /// Moves the element down.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <exception cref="System.ArgumentException">Nowhere to move element to!</exception>
        public static void MoveElementDown (this XElement element)
		{
			// Walk backwards until we find an element - ignore text nodes
			XNode nextNode = element.NextNode;
			while (nextNode != null && !(nextNode is XElement)) {
				nextNode = nextNode.NextNode;
			}
			if (nextNode == null) {
				throw new ArgumentException ("Nowhere to move element to!");
			}
			element.Remove ();
			nextNode.AddAfterSelf (element);
		}

        /// <summary>
        /// Gets attribute value or sets to default if absent.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value of the attribute or default value.</returns>
        public static XAttribute GetOrSetAttribute(this XElement element, string attributeName, string defaultValue)
        {
            XAttribute attribute = element.Attribute(attributeName);

            if (attribute == null) {
                attribute = new XAttribute(attributeName, defaultValue);
                element.Add(attribute);
            }

            return attribute;
        }

        /*public static IEnumerable<XElement> LocalDescendants(this XElement xElement, XName XNameLocal)
		{
			return xElement.Descendants("{" + xElement.GetDefaultNamespace().NamespaceName + "}" + XNameLocal.LocalName);
		}

		public static IEnumerable<XElement> LocalDescendants(this IEnumerable<XElement> xElements, XName XNameLocal)
		{
			return xElements.SelectMany(e => e.LocalDescendants(XNameLocal));
		}*/

        /*public static XAttribute LocalAttribute(this XElement xElement, XName XNameLocal, bool ignoreCase = false)
		{
			//return xElement.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(targetPropertyInfo.Name, StringComparison.CurrentCultureIgnoreCase));;
			return xElement.Attribute("{" + xElement.GetDefaultNamespace().NamespaceName + "}" + XNameLocal.LocalName);
		}*/

    }
}

#endif