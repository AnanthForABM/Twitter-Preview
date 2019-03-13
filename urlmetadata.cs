 static void Main(string[] args)
        {
            string responseBody;
            var request = (HttpWebRequest)WebRequest.Create("https://abmuat.emaplan.com/ABM/MediaServe/MediaLink?token=1b265fb1d4b54e098c47c1b284854cdb");

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, encoding))
                    responseBody = reader.ReadToEnd();
            }

            Regex metaregex = new Regex(@"<meta name=""twitter.+>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            Dictionary<string, string> metaAttributeList = new Dictionary<string, string>();
            foreach (Match metamatch in metaregex.Matches(responseBody))
            {
                string name = string.Empty;
                string value = string.Empty;

                Regex submetaregex =
                        new Regex(@"(?<name>\b(\w|-)+\b)\" +
                                  @"s*=\s*(""(?<value>" +
                                  @"[^""]*)""|'(?<value>[^']*)'" +
                                  @"|(?<value>[^""'<> ]+)\s*)+",
                                  RegexOptions.IgnoreCase |
                                  RegexOptions.ExplicitCapture);

                foreach (Match submetamatch in submetaregex.Matches(metamatch.Value.ToString()))
                {
                    if (("name" ==
                         submetamatch.Groups["name"].ToString().ToLower()))
                        name = submetamatch.Groups["value"].ToString();

                    if ("content" ==
                        submetamatch.Groups["name"].ToString().ToLower())
                    {
                        value = submetamatch.Groups["value"].ToString();
                        metaAttributeList.Add(name, value);
                    }
                }
            }
        }
    }
