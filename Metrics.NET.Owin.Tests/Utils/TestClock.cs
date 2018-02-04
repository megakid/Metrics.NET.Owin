﻿using System;
using Metrics.Utils;

namespace Metrics.NET.Owin.Tests.Utils
{
    public sealed class TestClock : Clock
    {
        private long nanoseconds = 0;

        public override long Nanoseconds { get { return this.nanoseconds; } }

        public override DateTime UTCDateTime { get { return new DateTime(this.nanoseconds / 100L, DateTimeKind.Utc); } }

        public void Advance(TimeUnit unit, long value)
        {
            this.nanoseconds += unit.ToNanoseconds(value);
            if (Advanced != null)
            {
                Advanced(this, EventArgs.Empty);
            }
        }

        public event EventHandler Advanced;
    }
}