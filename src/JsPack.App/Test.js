// Exporting individual features


// Exporting destructured assignments with renaming
export { variable1 as name1, variable2 as name2 };
export const { name1, name2: bar } = o;
export let name1, name2, nameN; // also var, const
export let name1 = 1, name2 = 3; // also var, const
export let name1;
export function functionName() { }
export class ClassName { }
export default { a, b }
export { xxx, yyyy };
export default function () { } // also class, function*
export default function name1() { } // also class, function*
export { name1 as default };
export default 12;

export * from "xxx";
export * as name1 from "xxx";

export { name1, name2 } from "xxx";
export { import1 as name1, import2 as name2 } from "xxx";
export { default } from "xxx";

import defaultExport from "module-name";
import * as name from "module-name";
import { export1 } from "module-name";
import { export1 as alias1 } from "module-name";
import { export1, export2 } from "module-name";
import { export1, export2 as alias2 } from "module-name";
import defaultExport, { export1 , export2 } from "module-name";
import defaultExport, * as name from "module-name";
import "module-name";
var promise = import("module-name");

