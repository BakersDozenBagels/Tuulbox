﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RT.Json;
using RT.PostBuild;
using RT.PropellerApi;
using RT.Serialization;
using RT.Servers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Tuulbox
{
    public sealed partial class TuulboxModule : IPropellerModule
    {
        public TuulboxSettings Settings;

        private static IEnumerable<ITuul> _toolsCache = getTools().ToArray();
        public static IEnumerable<ITuul> Tuuls { get { return _toolsCache; } }
        private UrlResolver _resolverCache = null;
        public UrlResolver Resolver
        {
            get
            {
                return _resolverCache ?? (_resolverCache = new UrlResolver(Tuuls.Select(generateUrlMapping)));
            }
        }

        private UrlMapping generateUrlMapping(ITuul tuul)
        {
            var resolver = new UrlResolver();
            resolver.Add(new UrlHook("", Settings.UseDomain, specific: true), handler: req => handle(req, tuul));
            return new UrlMapping(new UrlHook(tuul.UrlName ?? "", Settings.UseDomain), resolver.Handle);
        }

        private static IEnumerable<ITuul> getTools()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !type.IsAbstract && typeof(ITuul).IsAssignableFrom(type))
                .Select(type => (ITuul) Activator.CreateInstance(type))
                .Where(tuul => tuul.Enabled);
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            reportDuplicate(rep, t => t.Name, "name");
            reportDuplicate(rep, t => t.UrlName, "URL");

            string tuulWithNullUrl = null;
            foreach (var tuul in Tuuls)
            {
                if (tuul.UrlName == null)
                {
                    if (tuulWithNullUrl != null)
                        rep.Error(@"Two tuuls, “{0}” and “{1}”, have null UrlNames. Only one tuul can be the top-level tuul.".Fmt(tuul.UrlName, tuulWithNullUrl), "class " + tuul.GetType().Name);
                    else
                        tuulWithNullUrl = tuul.UrlName;
                }
                else if (tuul.UrlName.Length == 0)
                    rep.Error(@"The tuul “{0}” has an empty UrlName. To make it the top-level tuul, return null instead.".Fmt(tuul.UrlName), "class " + tuul.GetType().Name);
            }
        }

        private static Tuple<T, T> firstDuplicate<T, TCriterion>(IEnumerable<T> source, Func<T, TCriterion> criterion)
        {
            var dic = new Dictionary<TCriterion, T>();
            foreach (var item in source)
            {
                var cri = criterion(item);
                if (cri == null)
                    continue;
                if (dic.ContainsKey(cri))
                    return Tuple.Create(dic[cri], item);
                dic[cri] = item;
            }
            return null;
        }

        private static void reportDuplicate(IPostBuildReporter rep, Func<ITuul, string> criterion, string thing)
        {
            var duplicate = firstDuplicate(Tuuls, criterion);
            if (duplicate != null)
            {
                rep.Error(@"The tuul {0} ""{1}"" is used more than once.".Fmt(thing, criterion(duplicate.Item1)), "class " + duplicate.Item1.GetType().Name);
                rep.Error(@"... second use here.", "class " + duplicate.Item2.GetType().Name);
            }
        }

        public void Init(LoggerBase log, JsonValue settings, ISettingsSaver saver)
        {
            Settings = ClassifyJson.Deserialize<TuulboxSettings>(settings);
            if (Settings == null)
                Settings = new TuulboxSettings();
            saver.SaveSettings(ClassifyJson.Serialize(Settings));
        }

        public static byte[] FavIcon = Convert.FromBase64String(@"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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAA/AAAAaQAAAHoAAAB1AAAAZQAAAEQAAAAYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD///////8AAP///////wAA/wB/////AAD/gB////8AAP/AD////wAA/+AH////AAD/8Af///8AAP/4A////wAA3/wD////AADP/gH///8AAMf/Af///wAAw/8B////AADB/wH///8AAMD+Af///wAAwH4B////AADAOAH///8AAMAAAP///wAA4AAAf///AADgAAA///8AAPAAAB///wAA+AAAD///AAD+AAAH//8AAP+AAAP//wAA//+AAf//AAD//4AA//8AAP//4AAB/wAA///wAAB/AAD///gAAB8AAP///AAADwAA///+AAAHAAD///8AAAcAAP///4AAAwAA////gBwDAAD///+AfgMAAP///4B/AwAA////gP+BAAD///+A/8EAAP///4D/4wAA////gH/zAAD////AP/sAAP///8Af/wAA////4A//AAD////gB/8AAP////AD/wAA/////AH/AAD////+AP8AAP///////wAA////////AAA=");

        public string Name { get { return "Tuulbox"; } }
        public string[] FileFiltersToBeMonitoredForChanges { get { return null; } }
        public HttpResponse Handle(HttpRequest req)
        {
            if (req.Url.Path == "/favicon.ico")
                return HttpResponse.Create(FavIcon, "image/icon");
            return Resolver.Handle(req);
        }
        public bool MustReinitialize { get { return false; } }
        public void Shutdown() { }
    }
}