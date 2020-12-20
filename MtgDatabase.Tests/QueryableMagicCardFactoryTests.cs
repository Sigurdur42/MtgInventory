﻿using MtgDatabase.Models;
using NUnit.Framework;

namespace MtgDatabase.Tests
{
    [TestFixture]
    public partial class QueryableMagicCardFactoryTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
            _card = new QueryableMagicCard();
            _factory = new QueryableMagicCardFactory();
        }

        private QueryableMagicCard? _card;
        private QueryableMagicCardFactory? _factory;
    }
}