
print("test.lua")

-- Basic static functions
printDouble(3434.526)
printInt(34356)
printStr("Printing a string");
printThreeStrings("These","are","three strings");
printBool(true);
csGC()

-- Return C# objects
s1 = test:createSquarer("s1")
s2 = test:createSquarer("s2")
assert(s1 ~= s2)

assert(s1:getName() == "s1")
assert(s2:getName() == "s2")

assert(s1.square(5) == 25)
assert(s2.square(10) == 100)

-- Free s1
s1 = nil

t1 = getTest();
assert(t1 == test, "The same C# object should be equivalent in Lua")

-- Use the extra Lua context
test:eval("print('This is a test of test:eval')")

a = 3
test:eval("a = 99")
assert(a == 3)
test:eval("assert(a == 99)")

-- Test return values from Lua script
result = test:eval("return 7+4")
assert(result == 11, "test:eval(\"return 7+4\") is not 11?")

-- Test multiple return values from Lua script
print("The next lines should match")
print("1\t2\t3\t4\t5")
print(test:eval("return 1,2,3,4,5"));

-- Run the garbage collector once in a while
csGC();

-- Overload test
test:call("print", "This should print a string and the next line will be a number followed by true");
test:call("print", 5);
test:call("print", true);
test.call(test, "print", "This was called using . instead of :")

-- Returning objects
newtest = test:eval("return parent")
newtest:eval("objectTestInt = 66");
test:eval('assert(objectTestInt == 66, "Object was set in a different context? (" .. objectTestInt .. ")")');

test:eval('testFunc = function() return parent end');
newtest2 = test:call("testFunc");
assert(newtest2 == newtest, "ERROR, parent is different?");


-- error tests
print("Some errors should follow")

print("The following should fail because we use . instead of :")
test:eval('parent.call("print", "This should not print")')

print("Using . notation and passing in the wrong class as the first parameter")
test:eval("what = parent:createSquarer('what?')")
test:eval("what.getName(parent)")
-- s2.getName(test)

print("Passing wrong class as first param")
test:eval('parent.call(what, "print", "You do not see this")')

