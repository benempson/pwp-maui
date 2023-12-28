import { AppGlobals } from "./AppGlobals";
import { AppAlert } from "./Utils/AppAlert";
import { AppUtils } from "./Utils/AppUtils";

(window as any).App = {};
(window as any).App.AppGlobals = AppGlobals;
(window as any).App.AppAlert = AppAlert;
(window as any).App.AppUtils = AppUtils;