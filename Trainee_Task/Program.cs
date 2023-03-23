using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Trainee_Task
{
	class Program
	{
		static void Main(string[] args)
		{
			for (; ; )
			{
				try
				{
					//List for crawling website links
					List<string> crawlingWebSite = new List<string>();
					//List for sitemap.xml links
					List<string> siteMapXML = new List<string>();
					Console.WriteLine("Input URL: (https://example.com)");
					string inputURL = Console.ReadLine();
					//creating a resource identifier
					Uri uri = new Uri(inputURL);
					//getting https://
					string protocol = uri.Scheme;
					//getting domain name
					string domain = uri.Host;
					string finalURL = $"{protocol}://{domain}";
					Console.WriteLine();
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine("List with urls without sitemap.xml:");
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine();
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(finalURL);
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					StreamReader streamReader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
					string responseText = streamReader.ReadToEnd();
					Regex regex = new Regex("<a href=\"(.*?)\"");
					MatchCollection matches = regex.Matches(responseText);
					List<string> pages = new List<string>();
					foreach (Match match in matches)
					{
						string pageURL = match.Groups[1].Value;
						if (pageURL.StartsWith("http"))
							pages.Add(pageURL);
						else
							pages.Add(finalURL + pageURL);
					}
					streamReader.Close();
					response.Close();
					foreach (string link in pages)
					{
						HttpWebRequest linkRequest = (HttpWebRequest)WebRequest.Create(link);
						HttpWebResponse webResponse = (HttpWebResponse)linkRequest.GetResponse();
						StreamReader linkReader = new StreamReader(webResponse.GetResponseStream(), System.Text.Encoding.UTF8);
						string htmlCode = linkReader.ReadToEnd();
						linkReader.Close();
						webResponse.Close();
						//pattern to links
						string pattern = @"<a\s+(?:[^>]*?\s+)?href=([""'])(https?://\S+?(\.html)?)\1";
						Regex linkRegex = new Regex(pattern);
						//getting all links that fit the pattern
						foreach (Match match in linkRegex.Matches(htmlCode))
						{
							//timing of links
							Stopwatch stopwatch = new Stopwatch();
							//start
							stopwatch.Start();
							string links = match.Groups[2].Value;
							if(links.Contains(domain))
								//adding link to the list
								crawlingWebSite.Add(links);
							//stop
							stopwatch.Stop();
						}
					}
					//delete duplicate references
					List<string> distinctCrawling = crawlingWebSite.Distinct().ToList();
					int i = 1;
					foreach (string link in distinctCrawling)
					{
						Console.WriteLine($"{i}) {link}");
						i++;
					}
					//creating link to sitemap.xml
					XmlReader xmlReader = XmlReader.Create(finalURL + "/sitemap.xml");
					//adding links from loc-node to the list
					while (xmlReader.Read())
						if (xmlReader.Name == "loc")
							siteMapXML.Add(xmlReader.ReadInnerXml());
					xmlReader.Close();
					//founded in sitemap, but not founded after crawling
					IEnumerable<string> sitemapContains = siteMapXML.Except(crawlingWebSite);
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site: " + sitemapContains.Count());
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine();
					i = 1;
					foreach (string link in sitemapContains)
					{
						Console.WriteLine(i + ") " + link);
						i++;
					}
					//founded after crawling, but not founded in sitemap
					IEnumerable<string> crawlingContains = crawlingWebSite.Except(siteMapXML);
					Console.WriteLine();
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine("Urls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml: " + distinctCrawling.Count());
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine();
					i = 1;
					foreach (string link in crawlingContains)
					{
						Console.WriteLine(i + ") " + link);
						i++;
					}
					Console.WriteLine();
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine("Urls found after crawling a website: " + distinctCrawling.Count());
					Console.WriteLine("Urls found in sitemap: " + siteMapXML.Count());
					Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Console.WriteLine();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"{ex.Message}");
					Console.WriteLine("Try again\n");
				}
			}
		}
	}
}
