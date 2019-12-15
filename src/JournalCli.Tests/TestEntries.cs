using System;
using System.Collections.Generic;
using System.Text;

namespace JournalCli.Tests
{
    public static class TestEntries
    {
        public static string Empty = @"";

        public static string WithTags1 = @"---
tags:
  - blah
  - doh
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTags2 = @"---
tags:
  - blah
  - doh
  - cat
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTags3 = @"---
tags:
  - dog
  - cow
  - tree
  - forrest
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTags4 = @"---
tags:
  - horse
  - baby
  - blah
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTags5 = @"---
tags:
  - dog
  - carrot
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTags6 = @"---
tags:
  - hungry
  - doh
  - pig
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithTagsAndReadme = @"---
tags:
  - blah
  - doh
readme: 5 years
---
# This is a header!
This is some text.

# One last header
And more text";

        public static string WithoutTags = @"---
tags:
---
# This is a header!
This is some text.

## This is a secondary header.
This is more text.

# One last header
And more text";

        public static string WithoutFrontMatter = @"# This is a header!
This is some text. This is more text. And more text";
    }
}
