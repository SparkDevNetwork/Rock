#!/bin/sh
#

echo namespace Rock.Field.Types
echo {
for file in ../Field/Types/*.cs ../Field/SelectFromListFieldType.cs; do
  class=`grep -E -E ' class ([a-zA-Z\<\>]+) :' "$file" | grep 'FieldType' | sed -E 's/.* class ([a-zA-Z\<\>]+) :.*/\1/g'`
  echo "    public partial class $class : Rock.Field.FieldType"
  echo "    {"

  configuration=`cat "$file" | dos2unix | awk '/public static class ConfigurationKey/,/}/'`
  if [ -n "$configuration" ]; then
    echo "$configuration"
  fi

  configuration=`cat "$file" | dos2unix | awk '/public static class ConfigKey/,/}/'`
  if [ -n "$configuration" ]; then
    echo "$configuration"
  fi

  enums=`cat "$file" | dos2unix | awk '/public enum/,/}/'`
  if [ -n "$enums" ]; then
    echo "$enums"
  fi

  consts=`cat "$file" | dos2unix | grep -E '^        public (const|static readonly) string'`
  if [ -n "$consts" ]; then
    echo "$consts"
  fi

  echo "    }"
done
echo }
