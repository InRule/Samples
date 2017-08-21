using System;
using System.Collections.Generic;
using System.Linq;
using InRule.Repository;
using InRule.Runtime;

namespace RuleApplicationCache
{
    class Program
    {
        static void Main(string[] args)
        {
            ExplicitCache();
            ImplicitCache();
            ChangeCacheDepth();
            CacheRetention_NeverRetain();
            CacheRetention_AlwaysRetain();
            CacheRetention_FromWeight();
            CustomCachePolicy();
            PreCompileExecutableFunctions();
        }

        static void ExplicitCache()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- ExplicitCache()");
            Console.WriteLine("--------------------------------------------------------");

            // Ensure RuleApplication cache is empty
            RuleSession.RuleApplicationCache.Clear();
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Create in-memory RuleApplications
            RuleApplicationDef ruleAppDef1 = new RuleApplicationDef("MyApp1");
            ruleAppDef1.Entities.Add(new EntityDef("Entity1"));
            RuleApplicationDef ruleAppDef2 = new RuleApplicationDef("MyApp2");
            ruleAppDef2.Entities.Add(new EntityDef("Entity2"));

            // Explicit Cache Usage #1: Explicitly cache MyApp1 via RuleApplicationCache
            RuleApplicationReference ruleAppReference1 = RuleSession.RuleApplicationCache.Add(ruleAppDef1);
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1

            // Explicit Cache Usage #2: Explicitly cache MyApp2 via RuleApplicationReference.Compile()
            RuleApplicationReference ruleAppReference2 = new InMemoryRuleApplicationReference(ruleAppDef2);
            ruleAppReference2.Compile();
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 2

            // Consumption Usage #1: Create RuleSession with source RuleApplicationDef
            using (RuleSession session = new RuleSession(ruleAppDef1))
            {
                session.CreateEntity("Entity1");
            }
            using (RuleSession session = new RuleSession(ruleAppDef2))
            {
                session.CreateEntity("Entity2");
            }
            Console.WriteLine("4) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Still only expecting 2

            // Consumption Usage #2: Create RuleSession with RuleApplicationReference
            using (RuleSession session = new RuleSession(ruleAppReference1))
            {
                session.CreateEntity("Entity1");
            }
            using (RuleSession session = new RuleSession(ruleAppReference2))
            {
                session.CreateEntity("Entity2");
            }
            Console.WriteLine("5) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Still only expecting 2
        }

        static void ImplicitCache()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- ImplicitCache()");
            Console.WriteLine("--------------------------------------------------------");

            // Ensure RuleApplication cache is empty
            RuleSession.RuleApplicationCache.Clear();
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Implicitly cache RuleApplication
            using (RuleSession session = new RuleSession("NewRuleApplication.ruleappx"))
            {
                session.CreateEntity("Entity1");
            }
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1

            // Create RuleSession with implicitly cached RuleApplication using same RuleApplication source - Should not recompile
            using (RuleSession session = new RuleSession("NewRuleApplication.ruleappx"))
            {
                session.CreateEntity("Entity1");
            }
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Still only expecting 1
        }

        static void ChangeCacheDepth()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- ChangeCacheDepth()");
            Console.WriteLine("--------------------------------------------------------");

            // Set cache depth to 2 (Configuring cache policy automatically clears the cache)
            RuleSession.RuleApplicationCache.ConfigureCachePolicy(new DefaultRuleApplicationCachePolicy(2));
            Console.WriteLine("Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Create 3 in-memory RuleApplications
            RuleApplicationDef ra1 = new RuleApplicationDef("ra1");
            RuleApplicationDef ra2 = new RuleApplicationDef("ra2");
            RuleApplicationDef ra3 = new RuleApplicationDef("ra3");

            // Add each RuleApplication to the cache - Only 2 may fit so the oldest (ra1) will be expired
            RuleSession.RuleApplicationCache.Add(ra1);
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1
            RuleSession.RuleApplicationCache.Add(ra2);
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 2
            RuleSession.RuleApplicationCache.Add(ra3);
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 2

            // Reduce cache depth to 1
            RuleSession.RuleApplicationCache.ConfigureCachePolicy(new DefaultRuleApplicationCachePolicy(1));
            Console.WriteLine("4) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Add each RuleApplication to the cache - Only 1 may fit so the oldest (ra1+ra2) will be expired
            RuleSession.RuleApplicationCache.Add(ra1);
            Console.WriteLine("5) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1
            RuleSession.RuleApplicationCache.Add(ra2);
            Console.WriteLine("6) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1
            RuleSession.RuleApplicationCache.Add(ra3);
            Console.WriteLine("7) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1
        }

        static void CacheRetention_NeverRetain()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- CacheRetention_NeverRetain()");
            Console.WriteLine("--------------------------------------------------------");

            // Ensure RuleApplication cache is empty
            RuleSession.RuleApplicationCache.Clear();
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Usage #1 Attempt to explicitly add a RuleApplication to the cache with CacheRetention.NeverRetain via RuleApplicationCache.Add() - RuleApp is compiled, but not reusable
            RuleSession.RuleApplicationCache.Add("NewRuleApplication.ruleappx", CacheRetention.NeverRetain);
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Usage #2 Attempt to explicitly add a RuleApplication to the cache with CacheRetention.NeverRetain via RuleApplicationReference.Compile() - RuleApp is compiled, but not reusable
            RuleApplicationReference ruleAppReference = "NewRuleApplication.ruleappx";   // Note: RuleApplicationReference supports implicit casting from file path string, or RuleApplicationDef instance
            ruleAppReference.Compile(CacheRetention.NeverRetain);
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Usage #3 Attempt to implicitly add a RuleApplication to the cache with CacheRetention.NeverRetain via RuleSession() - RuleApp is compiled, but not reusable outside of RuleSession
            using (RuleSession session = new RuleSession("NewRuleApplication.ruleappx", CacheRetention.NeverRetain))
            {
                session.CreateEntity("Entity1");
            }
            Console.WriteLine("4) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0
        }

        static void CacheRetention_AlwaysRetain()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- CacheRetention_AlwaysRetain()");
            Console.WriteLine("--------------------------------------------------------");

            // Set cache depth to 1 (Configuring cache policy automatically clears the cache)
            RuleSession.RuleApplicationCache.ConfigureCachePolicy(new DefaultRuleApplicationCachePolicy(1));
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            RuleApplicationDef ruleApp1 = new RuleApplicationDef("ra1");

            // Add a RuleApplication to the cache with CacheRetention.Default
            RuleSession.RuleApplicationCache.Add(ruleApp1, CacheRetention.Default);
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 1

            // Add another RuleApplication to the cache with CacheRetention.AlwaysRetain - Should not expire existing RuleApplication even though cache depth is set to 1
            RuleSession.RuleApplicationCache.Add("NewRuleApplication.ruleappx", CacheRetention.AlwaysRetain);
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 2
        }

        static void CacheRetention_FromWeight()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- CacheRetention_FromWeight()");
            Console.WriteLine("--------------------------------------------------------");

            // Set cache depth to 3 (Configuring cache policy automatically clears the cache)
            RuleSession.RuleApplicationCache.ConfigureCachePolicy(new DefaultRuleApplicationCachePolicy(3));
            Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Create 4 RuleApplications
            RuleApplicationDef ra1 = new RuleApplicationDef("ra1");
            RuleApplicationDef ra2 = new RuleApplicationDef("ra2");
            RuleApplicationDef ra3 = new RuleApplicationDef("ra3");
            RuleApplicationDef ra4 = new RuleApplicationDef("ra4");

            // Add 4 RuleApplications to the cache, one has a lower weight
            RuleSession.RuleApplicationCache.Add(ra1, CacheRetention.FromWeight(1000));
            RuleSession.RuleApplicationCache.Add(ra2, CacheRetention.FromWeight(800));    // Lower weight means less likely to be retained/more likely to be expired
            RuleSession.RuleApplicationCache.Add(ra3, CacheRetention.FromWeight(1000));
            RuleSession.RuleApplicationCache.Add(ra4, CacheRetention.FromWeight(1000));
            Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 3

            // Show names of 3 cached RuleApplications - ra2 should be missing because adding ra4 should have expired it
            foreach (RuleApplicationReference cachedRuleApp in RuleSession.RuleApplicationCache.Items)
            {
                Console.WriteLine("Cached RuleApp: " + cachedRuleApp.GetRuleApplicationDef().Name);
            }



            // Ensure RuleApplication cache is empty
            RuleSession.RuleApplicationCache.Clear();
            Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Add 3 RuleApplications to the cache, ra2+ra3 have lower weights
            RuleSession.RuleApplicationCache.Add(ra1, CacheRetention.FromWeight(1000));
            RuleSession.RuleApplicationCache.Add(ra2, CacheRetention.FromWeight(400));
            RuleSession.RuleApplicationCache.Add(ra3, CacheRetention.FromWeight(400));
            Console.WriteLine("4) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 3

            // Show all 3 RuleApplications are currently cached
            int i = 5;
            foreach (RuleApplicationReference cachedRuleApp in RuleSession.RuleApplicationCache.Items)
            {
                Console.WriteLine(i++ + "Cached RuleApp: " + cachedRuleApp.GetRuleApplicationDef().Name);
            }

            // Use the oldest of the lower weighted RuleApplications in a RuleSession
            using (RuleSession session = new RuleSession(ra2))
            {
                session.ApplyRules();
            }
            Console.WriteLine("8) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 3

            // Add a fourth RuleApplication to the cache, which should expire the oldest lower-weighted RuleApplication (expires ra3 because ra2 was just used in a RuleSession)
            RuleSession.RuleApplicationCache.Add(ra4, CacheRetention.FromWeight(1000));
            Console.WriteLine("9) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 3

            // Show ra3 has been expired from the cache
            i = 10;
            foreach (RuleApplicationReference cachedRuleApp in RuleSession.RuleApplicationCache.Items)
            {
                Console.WriteLine(i++ + "Cached RuleApp: " + cachedRuleApp.GetRuleApplicationDef().Name);
            }
        }

        static void CustomCachePolicy()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- CustomCachePolicy()");
            Console.WriteLine("--------------------------------------------------------");

            // Set custom cache policy (Configuring cache policy automatically clears the cache)
            RuleSession.RuleApplicationCache.ConfigureCachePolicy(new MyCachePolicy());
            try
            {
                Console.WriteLine("1) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);                            // Expecting 0

                // Create 4 RuleApplications
                RuleApplicationDef ra1 = new RuleApplicationDef("ra1");
                RuleApplicationDef ra2 = new RuleApplicationDef("ra2");
                RuleApplicationDef ra3 = new RuleApplicationDef("ra3");
                RuleApplicationDef ra4 = new RuleApplicationDef("ra4");

                // Attempt to cache RuleApplication with invalid CacheRetention class
                RuleSession.RuleApplicationCache.Add(ra1, CacheRetention.Default);
                Console.WriteLine("2) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);                            // Expecting 0

                // Attempt to cache 2 RuleApplications whose CacheRetention have invalid names, and 1 that is valid
                RuleSession.RuleApplicationCache.Add(ra1, new MyCacheRetention("NOCACHE"));
                RuleSession.RuleApplicationCache.Add(ra2, new MyCacheRetention("INVALID"));
                RuleSession.RuleApplicationCache.Add(ra3, new MyCacheRetention("PerfectlyValidName"));
                Console.WriteLine("3) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);                            // Expecting 1
                Console.WriteLine("4) Cache RuleApp: " + RuleSession.RuleApplicationCache.Items.First().GetRuleApplicationDef().Name); // Expecting ra3

                // Attempt to cache another RuleApplication with same MyCacheRetention.Name - Should expire existing RuleApplication
                RuleSession.RuleApplicationCache.Add(ra4, new MyCacheRetention("PerfectlyValidName"));
                Console.WriteLine("5) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);                            // Expecting 1
                Console.WriteLine("6) Cache RuleApp: " + RuleSession.RuleApplicationCache.Items.First().GetRuleApplicationDef().Name); // Expecting ra4

                // Add another RuleApplication to cache with different MyCacheRetention.Name
                RuleSession.RuleApplicationCache.Add(ra1, new MyCacheRetention("AnotherPerfectlyName"));
                Console.WriteLine("7) Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);                            // Expecting 2
            }
            finally
            {
                // Reset cache to default policy
                RuleSession.RuleApplicationCache.ConfigureCachePolicy(new DefaultRuleApplicationCachePolicy());
            }
        }

        static void PreCompileExecutableFunctions()
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("--- PreCompileExecutableFunctions()");
            Console.WriteLine("--------------------------------------------------------");

            // Ensure RuleApplication cache is empty
            RuleSession.RuleApplicationCache.Clear();
            Console.WriteLine("1)  Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 0

            // Create 4 in-memory RuleApplications
            RuleApplicationDef ra1 = new RuleApplicationDef("ra1");
            ra1.Entities.Add(new EntityDef("Entity1")).Fields.Add(new FieldDef("Calculation1", "1 + 2", DataType.Integer));
            RuleApplicationDef ra2 = new RuleApplicationDef("ra2");
            ra2.Entities.Add(new EntityDef("Entity1")).Fields.Add(new FieldDef("Calculation1", "1 + 2", DataType.Integer));
            RuleApplicationDef ra3 = new RuleApplicationDef("ra3");
            ra3.Entities.Add(new EntityDef("Entity1")).Fields.Add(new FieldDef("Calculation1", "1 + 2", DataType.Integer));
            RuleApplicationDef ra4 = new RuleApplicationDef("ra4");
            ra4.Entities.Add(new EntityDef("Entity1")).Fields.Add(new FieldDef("Calculation1", "1 + 2", DataType.Integer));

            // Create 4 RuleApplicationReferences - Note the implicit casting
            RuleApplicationReference raRef1 = ra1;
            RuleApplicationReference raRef2 = ra2;
            RuleApplicationReference raRef3 = ra3;
            RuleApplicationReference raRef4 = ra4;

            // Usage #1: Pre-compile RuleApplications via RuleApplicationCache.Add() - Note EngineLogOptions are compiled into the executable functions
            raRef1.Compile(CompileSettings.Create(EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics));
            raRef2.Compile();
            raRef3.Compile(CompileSettings.Create(EngineLogOptions.SummaryStatistics));
            raRef4.Compile(CompileSettings.Default);
            Console.WriteLine("2)  Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting 4

            // Usage #2: Pre-compile RuleApplications via RuleApplicationReference.Compile()
            RuleSession.RuleApplicationCache.Add(ra1, CompileSettings.Create(EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics));
            RuleSession.RuleApplicationCache.Add(ra2);
            RuleSession.RuleApplicationCache.Add(ra3, CompileSettings.Create(EngineLogOptions.SummaryStatistics));
            RuleSession.RuleApplicationCache.Add(ra4, CompileSettings.Default);
            Console.WriteLine("3)  Cached RuleApplications: " + RuleSession.RuleApplicationCache.Count);    // Expecting same 4

            // Test ra1 - Metadata + Functions pre-compiled
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("4)  RuleApplication '{0}' Metadata Compile timestamp: {1}", ra1.Name, raRef1.LastMetadataCompile);
            Console.WriteLine("5)  RuleApplication '{0}' Function Compile timestamp: {1}", ra1.Name, raRef1.GetLastFunctionCompile(CompileSettings.Create(EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics)));
            using (RuleSession session = new RuleSession(raRef1))
            {
                session.Settings.LogOptions = EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics;
                session.CreateEntity("Entity1");
                session.ApplyRules();
                Console.WriteLine("6) RuleApplication '{0}' Dynamic Function Compile Count during rule execution: {1}", ra1.Name, session.Statistics.FunctionCompileTime.SampleCount); // Expecting 0 due to function pre-compile
            }

            // Test ra2 - Only Metadata pre-compiled
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("7)  RuleApplication '{0}' Metadata Compile timestamp: {1}", ra2.Name, raRef2.LastMetadataCompile);
            Console.WriteLine("8)  RuleApplication '{0}' Function Compile timestamp: {1}", ra2.Name, raRef2.GetLastFunctionCompile(CompileSettings.Create(EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics)));
            using (RuleSession session = new RuleSession(raRef2))
            {
                session.Settings.LogOptions = EngineLogOptions.Execution | EngineLogOptions.SummaryStatistics;
                session.CreateEntity("Entity1");
                session.ApplyRules();
                Console.WriteLine("9)  RuleApplication '{0}' Dynamic Function Compile Count during rule execution: {1}", ra2.Name, session.Statistics.FunctionCompileTime.SampleCount); // Expecting 1 due to functions not pre-compile
            }

            // Test ra3 - Metadata + Functions pre-compiled
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("10) RuleApplication '{0}' Metadata Compile timestamp: {1}", ra3.Name, raRef3.LastMetadataCompile);
            Console.WriteLine("11) RuleApplication '{0}' Function Compile timestamp: {1}", ra3.Name, raRef3.GetLastFunctionCompile(CompileSettings.Create(EngineLogOptions.SummaryStatistics)));
            using (RuleSession session = new RuleSession(raRef3))
            {
                session.Settings.LogOptions = EngineLogOptions.SummaryStatistics;
                session.CreateEntity("Entity1");
                session.ApplyRules();
                Console.WriteLine("12) RuleApplication '{0}' Dynamic Function Compile Count during rule execution: {1}", ra3.Name, session.Statistics.FunctionCompileTime.SampleCount); // Expecting 0 due to function pre-copile
            }

            // Test ra4 - Metadata + Functions pre-compiled
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("13) RuleApplication '{0}' Metadata Compile timestamp: {1}", ra4.Name, raRef4.LastMetadataCompile);
            Console.WriteLine("14) RuleApplication '{0}' Function Compile timestamp (Default): {1}", ra4.Name, raRef4.GetLastFunctionCompile(CompileSettings.Default));
            // Note: RuleSession Statistics requires EngineLogOptions.SummaryStatistics to be enabled; CompileSettings.Default does not enable SummaryStatistics unless Info logging is enabled in the .config file.

            // Test ra4 Function Compile timestamp for different type of CompileSettings
            Console.WriteLine("15) RuleApplication '{0}' Function Compile timestamp (EngineLogOptions.Execution): {1}", ra4.Name, raRef4.GetLastFunctionCompile(CompileSettings.Create(EngineLogOptions.Execution)));    // Should be missing because we only checked compiled functions for the Default options
        }
    }

    public class MyCacheRetention : CacheRetention
    {
        public MyCacheRetention(string name) : base(CacheRetention.Default.Weight)
        {
            Name = name;
        }

        public string Name { get; private set; }

        // *** Note: It is important to override the .Equals() method to compare any custom properties, otherwise the cache will not function correctly
        public override bool Equals(object obj)
        {
            if (!(obj is MyCacheRetention)) return false;
            return ((MyCacheRetention)obj).Name == this.Name;
        }

        public override int GetHashCode()
        {
            return Name == null ? 0 : Name.GetHashCode();
        }
    }

    public class MyCachePolicy : IRuleApplicationCachePolicy
    {
        // This policy does not have a maximum depth and does not use the CacheRetention.Weight - It only checks the property MyCacheRetention.Name
        public bool AllowCacheEntry(RuleApplicationReference candidate, IEnumerable<RuleApplicationReference> cachedItems, out IEnumerable<RuleApplicationReference> itemsToExpire)
        {
            // If cache retention is not our own MyCacheRetention, don't cache
            MyCacheRetention retention = candidate.CacheRetention as MyCacheRetention;
            if (retention == null)
            {
                itemsToExpire = null;
                return false;
            }

            // Do not cache specific retention names
            if (retention.Name == "NOCACHE" || retention.Name == "INVALID")
            {
                itemsToExpire = null;
                return false;
            }

            // Identify cached items to expire - Expire any RuleApplication whose MyCacheRetention has duplicate name as candidate
            itemsToExpire = cachedItems.Where(item => ((MyCacheRetention)item.CacheRetention).Name == retention.Name);
            return true;
        }
    }

}
