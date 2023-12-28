import { AppGlobals } from '../AppGlobals';
export class AppUtils {
    public static HasErrored = false;

    private static _errorHeader = 'There was an unexpected error, please contact Support with the following information:';

    public static CopyToClipboardFromElement(elementId: string): void {
        const contentElement = document.getElementById(elementId);
        const content = contentElement?.innerText;
        ((window as any).top as any).navigator.clipboard.writeText(content);
    }

    /**
     * Display an error message to the user
     * @param msg The error message to display
     * @param e The caught exception (if any)
     */
    public static ErrorMessage(msg: string, e: any) {
        AppUtils.HasErrored = true;
        msg = AppUtils.ExtractError(msg, e);

        try {
            AppUtils.Log('*** ERROR:\r\n\r\n' + msg, true);
            const finalMsg: string = msg.replace(/\r\n/g, '<br />\rz').replace(/\n/g, '<br />z').replace(/\z/g, '\n');

            //if in unit test environment PWPAlert.jQueryLocal will be undefined
            // PWPAlert.TSAJQ();
            // if (typeof PWPAlert.jQueryLocal === AppGlobals.STR_FUNCTION)
            //     PWPAlert.showWithSupportInfo('Error', finalMsg, msg, PWPAlert.IconEnum.ERROR);
            // else
            alert(finalMsg);
        } catch (e1) {
            AppUtils.Log('*** ERROR in AppUtils.ErrorMessage', e1, true);
            AppUtils.Log('*** ORIGINAL ERROR:\r\n\r\n' + msg, true);

            //consider that in unit tests alert() does not exist
            if (typeof alert === AppGlobals.STR_FUNCTION) alert(msg);
        }
    }

    /**
     * Extract the error stack and compile the full error message
     * @param msg The user-aimed error message
     * @param e The error received
     */
    public static ExtractError(msg: string, e: any) {
        msg = AppUtils._errorHeader + '\r\n\r\n*** ERROR MESSAGE:\r\n' + msg;
        const errDetails = AppUtils.GetExceptionDetails(e);
        msg = msg + '\r\n\r\n*** SUPPORT INFO: \r\n' + errDetails;
        return msg;
    }

    /**
     * Get a string representing the exception details for logging / display to user
     * @param e The exception from which to get the details
     * @returns
     */
    public static GetExceptionDetails(e: any): string | null {
        const str_description: string = 'description';
        const str_message: string = 'message';
        const str_stack: string = 'stack';

        //the following 2 fields are used in the ComponentFramework WebApi
        //https://powerusers.microsoft.com/t5/Power-Apps-Pro-Dev-ISV/How-to-call-custom-actions-using-PCF/m-p/370411/highlight/true#M990
        const str_status: string = 'status';
        const str_status_text: string = 'statusText';

        let details: string | null = null;

        if (e && typeof e === AppGlobals.STR_OBJECT) {
            if (e.hasOwnProperty(str_stack) && e[str_stack] !== null) details = e[str_stack];
            else if (e.hasOwnProperty(str_message) && e[str_message] !== null) details = e[str_message];
            else if (e.hasOwnProperty(str_description) && e[str_description] !== null) details = e[str_description];
            else if (e.hasOwnProperty(str_status) && e[str_status] !== null) {
                details = e[str_status];
                if (e.hasOwnProperty(str_status_text) && e[str_status_text] !== null) details += ': ' + e[str_status_text];
            }
        }

        return details;
    }

    /**
     * Log a message to the browser console. Messages will not be output if the AppUtils.debug is false (unless you use the force param)
     * @param msg The message to log
     * @param object The object associated with the message - will be stringified and output also (not required)
     * @param force Whether or not to override the AppUtils.debug variable and force output of the message
     * @param logPrefix Add a custom log prefix to the log, overriding the default log prefix held in Globals
     */
    public static Log(msg: any, object?: any, force?: boolean, logPrefix?: string) {
        try {
            let objectIsReported: boolean = false;

            // function overloading - if we want to force and not pass an object, force is the 2nd parameter...
            if (typeof object === 'boolean' && (typeof force === AppGlobals.STR_UNDEFINED || force === null)) {
                force = object;
                object = null;
            }

            //https://stackoverflow.com/a/69198602/206852 - keyof typeof syntax
            //@ts-ignore
            if (typeof window[AppUtils.CONSOLE as keyof typeof Window] !== AppGlobals.STR_UNDEFINED && (AppUtils.debug || force)) {
                //BE 210920: If the message is an exception, display the exception stack if there is one
                let exception: string | null = AppUtils.GetExceptionDetails(msg);
                if (exception) msg = exception;

                //BE 210921: If we have an exception passed in as the object, add it to the message
                exception = AppUtils.GetExceptionDetails(object);
                if (exception) {
                    msg = msg + '\r\n\r\n*** EXCEPTION: \r\n' + exception;
                    objectIsReported = true;
                }

                //BE 210920: Add the Global log prefix to the message
                const tempMsg = msg;
                if (logPrefix !== null && typeof logPrefix !== AppGlobals.STR_UNDEFINED) msg = logPrefix + ': ';
                else if (AppGlobals.hasOwnProperty(AppGlobals.STR_LOG_PREFIX)) msg = AppGlobals.LOG_PREFIX + ': ';

                const currentDate = new Date();
                const time = currentDate.getHours().toString().padStart(2, '0') + ':' + currentDate.getMinutes().toString().padStart(2, '0') + ':' + currentDate.getSeconds().toString().padStart(2, '0');
                msg = time + '|: ' + msg + tempMsg;
                console.log(msg);

                if (objectIsReported === false && object) {
                    console.log(object);
                    //const stringified: string | null = AppUtils.StringifyObject(object);
                    //if (stringified)
                    //{
                    //    console.log(stringified);
                    //}
                }
            }
        } catch (e) {
            const errMsg = AppUtils.GetExceptionDetails(e);
            console.log('*** ERROR in AppUtils.Log:\r\n\r\n' + errMsg);
        }
    }
}
